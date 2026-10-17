using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public class InteraksiRow
{
    public string Tanggal { get; init; } = "";
    public string Media { get; init; } = "";
    public string Ringkasan { get; init; } = "";
    public string Oleh { get; init; } = "";
    public string FollowUpText { get; init; } = "";
    public bool HasFollowUp { get; init; }
}

public class RiwayatRow
{
    public string Text { get; init; } = "";
    public string Waktu { get; init; } = "";
}

public partial class PengajuanDetailViewModel : ViewModelBase
{
    private readonly INavigator _nav;
    private readonly int _id;
    private PengajuanItem? _item;

    public PengajuanDetailViewModel(AppServices services, INavigator nav, int id) : base(services)
    {
        _nav = nav;
        _id = id;
    }

    // Header
    [ObservableProperty] private string _perusahaan = "";
    [ObservableProperty] private string _statusLabel = "";
    [ObservableProperty] private IBrush _statusBg = Palette.Gray;
    [ObservableProperty] private IBrush _statusFg = Palette.GrayFg;
    [ObservableProperty] private string _subtitle = "";
    [ObservableProperty] private string _kontak = "";
    [ObservableProperty] private string _jadwal = "";
    [ObservableProperty] private string _alasanText = "";
    [ObservableProperty] private bool _hasAlasan;

    [ObservableProperty] private List<InteraksiRow> _interaksi = new();
    [ObservableProperty] private List<RiwayatRow> _riwayat = new();

    // Form interaksi
    public List<string> MediaOptions { get; } = Validator.MediaOptions.ToList();
    [ObservableProperty] private string _formMedia = "WhatsApp";
    [ObservableProperty] private string _formRingkasan = "";
    [ObservableProperty] private DateTime? _formFollowUp;

    // Ubah status
    [ObservableProperty] private List<Opt<StatusPengajuan>> _statusOptions = new();
    [ObservableProperty] private Opt<StatusPengajuan>? _selectedTarget;
    [ObservableProperty] private List<AlasanPenolakan> _alasanOptions = new();
    [ObservableProperty] private AlasanPenolakan? _selectedAlasan;
    [ObservableProperty] private bool _canChangeStatus;

    public bool NeedsReason => SelectedTarget is not null && StatusRules.RequiresReason(SelectedTarget.Value);
    partial void OnSelectedTargetChanged(Opt<StatusPengajuan>? value) => OnPropertyChanged(nameof(NeedsReason));

    public bool NoInteraksi => Interaksi.Count == 0;
    partial void OnInteraksiChanged(List<InteraksiRow> value) => OnPropertyChanged(nameof(NoInteraksi));

    public override async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var item = await S.Pengajuan.GetAsync(_id) ?? throw new DomainException("Pengajuan tidak ditemukan.");
            _item = item;
            Perusahaan = item.Perusahaan;
            StatusLabel = item.Status.ToLabel();
            (StatusBg, StatusFg) = Palette.ForStatus(item.Status);
            Subtitle = $"{item.BidangIndustri}, {item.Kota ?? "-"} | {item.Periode} | PIC internal: {item.PicUsername}";
            Kontak = $"{item.NamaPIC ?? "-"} ({item.JabatanPIC ?? "-"}), {item.Email ?? "-"}, {item.Telepon ?? "-"}";
            Jadwal = $"Usulan tanggal: {Fmt.Date(item.TanggalUsulan)} | Kuota: {item.KuotaPeserta?.ToString() ?? "-"} | Surat dikirim: {Fmt.Date(item.TanggalKirimSurat)}";
            HasAlasan = item.AlasanPenolakan is not null;
            AlasanText = item.AlasanPenolakan is null ? "" : "Alasan: " + item.AlasanPenolakan;

            var inter = await S.Pengajuan.ListInteraksiAsync(_id);
            Interaksi = inter.Select(i => new InteraksiRow
            {
                Tanggal = Fmt.DateTimeShort(i.Tanggal), Media = i.Media, Ringkasan = i.Ringkasan, Oleh = i.UserName,
                HasFollowUp = i.TanggalFollowUp is not null,
                FollowUpText = i.TanggalFollowUp is null ? "" : "Follow-up: " + Fmt.Date(i.TanggalFollowUp) + (i.IsSelesai ? " (selesai)" : "")
            }).ToList();

            var rw = await S.Pengajuan.ListRiwayatAsync(_id);
            Riwayat = rw.Select(r => new RiwayatRow
            {
                Text = (r.StatusLama is null ? "Dibuat sebagai " : r.StatusLama.Value.ToLabel() + " ke ") + r.StatusBaru.ToLabel() + " oleh " + r.UserName,
                Waktu = Fmt.DateTimeShort(r.Waktu)
            }).ToList();

            var options = StatusRules.NextOptions(item.Status).Select(s => new Opt<StatusPengajuan>(s, s.ToLabel())).ToList();
            StatusOptions = options;
            SelectedTarget = options.FirstOrDefault();
            CanChangeStatus = CanWrite && options.Count > 0;
            AlasanOptions = await S.Master.ListAlasanAsync();
            SelectedAlasan = null;
        });
    }

    [RelayCommand]
    private Task BackAsync() => _nav.NavigateAsync("pengajuan");

    [RelayCommand]
    private async Task AddInteraksiAsync()
    {
        if (_item is null) return;
        bool ok = await RunAsync(async () =>
        {
            var input = new InteraksiInput { Media = FormMedia, Ringkasan = FormRingkasan, TanggalFollowUp = Fmt.ToDateOnly(FormFollowUp) };
            await S.Pengajuan.AddInteraksiAsync(_item, input);
        }, "Komunikasi dicatat.");
        if (ok)
        {
            FormRingkasan = "";
            FormFollowUp = null;
            await LoadAsync();
            await _nav.RefreshBadgeAsync();
        }
    }

    [RelayCommand]
    private async Task ChangeStatusAsync()
    {
        if (_item is null || SelectedTarget is null) return;
        bool ok = await RunAsync(
            () => S.Pengajuan.ChangeStatusAsync(_item, SelectedTarget.Value, SelectedAlasan?.AlasanId),
            "Status diperbarui.");
        if (ok)
        {
            await LoadAsync();
            await _nav.RefreshBadgeAsync();
        }
    }
}
