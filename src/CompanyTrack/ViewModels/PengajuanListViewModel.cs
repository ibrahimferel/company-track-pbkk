using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public partial class PengajuanRow : ObservableObject
{
    public PengajuanItem Item { get; }
    public string Perusahaan => Item.Perusahaan;
    public string Industri => Item.BidangIndustri;
    public string StatusLabel => Item.Status.ToLabel();
    public IBrush StatusBg { get; }
    public IBrush StatusFg { get; }
    public string Pic => Item.PicUsername;
    public string LastActivity { get; }
    public bool IsStale { get; }
    public string NextFollowUp { get; }
    public bool CanWrite { get; }

    [ObservableProperty] private bool _isConfirmingDelete;

    public IRelayCommand OpenCommand { get; }
    public IRelayCommand EditCommand { get; }
    public IRelayCommand AskDeleteCommand { get; }
    public IRelayCommand CancelDeleteCommand { get; }
    public IRelayCommand ConfirmDeleteCommand { get; }

    public PengajuanRow(PengajuanItem item, bool canWrite, int threshold,
        Action<PengajuanRow> open, Action<PengajuanRow> edit, Action<PengajuanRow> delete)
    {
        Item = item;
        CanWrite = canWrite;
        (StatusBg, StatusFg) = Palette.ForStatus(item.Status);
        int days = StaleChecker.DaysSince(item.TanggalAktivitasTerakhir, DateTime.Now);
        LastActivity = days == 0 ? "Hari ini" : $"{days} hari lalu";
        IsStale = StaleChecker.IsStale(item.Status, item.TanggalAktivitasTerakhir, DateTime.Now, threshold);
        NextFollowUp = item.NextFollowUp is null ? "-" : Fmt.Date(item.NextFollowUp);
        OpenCommand = new RelayCommand(() => open(this));
        EditCommand = new RelayCommand(() => edit(this));
        AskDeleteCommand = new RelayCommand(() => IsConfirmingDelete = true);
        CancelDeleteCommand = new RelayCommand(() => IsConfirmingDelete = false);
        ConfirmDeleteCommand = new RelayCommand(() => delete(this));
    }
}

public partial class PengajuanListViewModel : ViewModelBase
{
    private readonly INavigator _nav;
    private bool _loading;
    private int _threshold = StaleChecker.DefaultThresholdDays;
    private List<Perusahaan> _perusahaan = new();
    private PengajuanRow? _editing;

    public PengajuanListViewModel(AppServices services, INavigator nav) : base(services) => _nav = nav;

    // Filter
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private Opt<StatusPengajuan?>? _selectedStatus;
    [ObservableProperty] private Opt<int?>? _selectedPic;
    [ObservableProperty] private Opt<int?>? _selectedPeriode;
    [ObservableProperty] private Opt<string?>? _selectedIndustri;

    public List<Opt<StatusPengajuan?>> StatusOptions { get; private set; } = new();
    public List<Opt<int?>> PicOptions { get; private set; } = new();
    public List<Opt<int?>> PeriodeOptions { get; private set; } = new();
    public List<Opt<string?>> IndustriOptions { get; private set; } = new();

    [ObservableProperty] private List<PengajuanRow> _rows = new();
    [ObservableProperty] private string _countText = "";

    // Form
    public List<Opt<int>> FormPerusahaanOptions { get; private set; } = new();
    public List<Opt<int>> FormPeriodeOptions { get; private set; } = new();
    public List<Opt<int>> FormPicOptions { get; private set; } = new();

    [ObservableProperty] private bool _isFormOpen;
    [ObservableProperty] private bool _isNew = true;
    [ObservableProperty] private string _formTitle = "";
    [ObservableProperty] private Opt<int>? _formPerusahaan;
    [ObservableProperty] private Opt<int>? _formPeriode;
    [ObservableProperty] private Opt<int>? _formPic;
    [ObservableProperty] private DateTime? _formTanggalUsulan;
    [ObservableProperty] private DateTime? _formTanggalSurat;
    [ObservableProperty] private string _formKuota = "";

    public bool IsEmpty => Rows.Count == 0;
    partial void OnRowsChanged(List<PengajuanRow> value) => OnPropertyChanged(nameof(IsEmpty));

    partial void OnSearchTextChanged(string value) => _ = ReloadAsync();
    partial void OnSelectedStatusChanged(Opt<StatusPengajuan?>? value) => _ = ReloadAsync();
    partial void OnSelectedPicChanged(Opt<int?>? value) => _ = ReloadAsync();
    partial void OnSelectedPeriodeChanged(Opt<int?>? value) => _ = ReloadAsync();
    partial void OnSelectedIndustriChanged(Opt<string?>? value) => _ = ReloadAsync();

    public override async Task LoadAsync()
    {
        _loading = true;
        await RunAsync(async () =>
        {
            _threshold = await S.Dashboard.ThresholdAsync();
            var periodes = await S.Master.ListPeriodeAsync();
            var pics = await S.Auth.ListPicAsync();
            var industri = await S.Master.ListIndustriAsync();
            _perusahaan = await S.Master.ListPerusahaanAsync();

            StatusOptions = new List<Opt<StatusPengajuan?>> { new(null, "Semua status") };
            StatusOptions.AddRange(Enum.GetValues<StatusPengajuan>().Select(s => new Opt<StatusPengajuan?>(s, s.ToLabel())));
            PicOptions = new List<Opt<int?>> { new(null, "Semua PIC") };
            PicOptions.AddRange(pics.Select(u => new Opt<int?>(u.UserId, u.Username)));
            PeriodeOptions = new List<Opt<int?>> { new(null, "Semua periode") };
            PeriodeOptions.AddRange(periodes.Select(p => new Opt<int?>(p.PeriodeId, p.Nama)));
            IndustriOptions = new List<Opt<string?>> { new(null, "Semua industri") };
            IndustriOptions.AddRange(industri.Select(i => new Opt<string?>(i, i)));

            FormPerusahaanOptions = _perusahaan.Select(p => new Opt<int>(p.PerusahaanId, p.Nama)).ToList();
            FormPeriodeOptions = periodes.Select(p => new Opt<int>(p.PeriodeId, p.Nama)).ToList();
            FormPicOptions = pics.Select(u => new Opt<int>(u.UserId, $"{u.Nama} ({u.Username})")).ToList();

            OnPropertyChanged(nameof(StatusOptions));
            OnPropertyChanged(nameof(PicOptions));
            OnPropertyChanged(nameof(PeriodeOptions));
            OnPropertyChanged(nameof(IndustriOptions));
            OnPropertyChanged(nameof(FormPerusahaanOptions));
            OnPropertyChanged(nameof(FormPeriodeOptions));
            OnPropertyChanged(nameof(FormPicOptions));

            SelectedStatus = StatusOptions[0];
            SelectedPic = PicOptions[0];
            SelectedIndustri = IndustriOptions[0];
            var active = periodes.FirstOrDefault(p => p.IsAktif);
            SelectedPeriode = PeriodeOptions.FirstOrDefault(o => active is not null && o.Value == active.PeriodeId) ?? PeriodeOptions[0];
        });
        _loading = false;
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (_loading) return;
        await RunAsync(async () =>
        {
            var filter = new PengajuanFilter
            {
                Search = SearchText,
                Status = SelectedStatus?.Value,
                PicUserId = SelectedPic?.Value,
                PeriodeId = SelectedPeriode?.Value,
                Industri = SelectedIndustri?.Value
            };
            var items = await S.Pengajuan.QueryAsync(filter);
            Rows = items.Select(i => new PengajuanRow(i, CanWrite, _threshold, OpenRow, EditRow, DeleteRowAsync)).ToList();
            CountText = $"{Rows.Count} pengajuan";
        });
    }

    private void OpenRow(PengajuanRow row) => _ = _nav.OpenDetailAsync(row.Item.PengajuanId);

    [RelayCommand]
    private void NewPengajuan()
    {
        if (!CanWrite) return;
        _editing = null;
        IsNew = true;
        FormTitle = "Tambah pengajuan";
        FormPerusahaan = null;
        FormPeriode = FormPeriodeOptions.FirstOrDefault(o => o.Value == SelectedPeriode?.Value) ?? FormPeriodeOptions.FirstOrDefault();
        FormPic = FormPicOptions.FirstOrDefault(o => o.Value == S.Session.CurrentUser?.UserId);
        FormTanggalUsulan = null;
        FormTanggalSurat = null;
        FormKuota = "";
        IsFormOpen = true;
    }

    private void EditRow(PengajuanRow row)
    {
        if (!CanWrite) return;
        _editing = row;
        var i = row.Item;
        IsNew = false;
        FormTitle = "Ubah pengajuan: " + i.Perusahaan;
        FormPerusahaan = FormPerusahaanOptions.FirstOrDefault(o => o.Value == i.PerusahaanId);
        FormPeriode = FormPeriodeOptions.FirstOrDefault(o => o.Value == i.PeriodeId);
        FormPic = FormPicOptions.FirstOrDefault(o => o.Value == i.PicUserId);
        FormTanggalUsulan = Fmt.ToDateTime(i.TanggalUsulan);
        FormTanggalSurat = Fmt.ToDateTime(i.TanggalKirimSurat);
        FormKuota = i.KuotaPeserta?.ToString() ?? "";
        IsFormOpen = true;
    }

    [RelayCommand]
    private void CancelForm() => IsFormOpen = false;

    [RelayCommand]
    private async Task SaveFormAsync()
    {
        bool ok = await RunAsync(async () =>
        {
            int? kuota = null;
            if (!string.IsNullOrWhiteSpace(FormKuota))
            {
                if (!int.TryParse(FormKuota.Trim(), out int k)) throw new DomainException("Kuota peserta harus berupa angka.");
                kuota = k;
            }
            var input = new PengajuanInput
            {
                PerusahaanId = FormPerusahaan?.Value ?? 0,
                PeriodeId = FormPeriode?.Value ?? 0,
                PicUserId = FormPic?.Value ?? 0,
                TanggalUsulan = Fmt.ToDateOnly(FormTanggalUsulan),
                TanggalKirimSurat = Fmt.ToDateOnly(FormTanggalSurat),
                KuotaPeserta = kuota
            };
            if (_editing is null) await S.Pengajuan.CreateAsync(input, FormPerusahaan?.Label ?? "");
            else await S.Pengajuan.UpdateAsync(_editing.Item, input);
        }, "Pengajuan disimpan.");
        if (ok)
        {
            IsFormOpen = false;
            await ReloadAsync();
        }
    }

    private async void DeleteRowAsync(PengajuanRow row)
    {
        bool ok = await RunAsync(() => S.Pengajuan.DeleteAsync(row.Item), "Pengajuan dihapus.");
        if (ok) await ReloadAsync();
        else row.IsConfirmingDelete = false;
    }
}
