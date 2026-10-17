using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public class PeriodeRow
{
    public PeriodeKunjungan Item { get; init; } = null!;
    public string Nama => Item.Nama;
    public string Tanggal => Fmt.Date(Item.TanggalKunjungan);
    public string Target => Item.TargetPerusahaan + " perusahaan";
    public string StatusText => Item.IsAktif ? "Aktif" : "Arsip";
    public bool CanManage { get; init; }
    public IRelayCommand EditCommand { get; init; } = null!;
}

public partial class PeriodeViewModel : ViewModelBase
{
    private int _editId;

    public PeriodeViewModel(AppServices services) : base(services) { }

    [ObservableProperty] private List<PeriodeRow> _rows = new();
    [ObservableProperty] private bool _isFormOpen;
    [ObservableProperty] private string _formTitle = "";
    [ObservableProperty] private string _formNama = "";
    [ObservableProperty] private DateTime? _formTanggal;
    [ObservableProperty] private string _formTarget = "";
    [ObservableProperty] private bool _formAktif;

    public override async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var list = await S.Master.ListPeriodeAsync();
            Rows = list.Select(p => new PeriodeRow { Item = p, CanManage = CanManage, EditCommand = new RelayCommand(() => EditRow(p)) }).ToList();
        });
    }

    [RelayCommand]
    private void NewPeriode()
    {
        if (!CanManage) return;
        _editId = 0;
        FormTitle = "Tambah periode";
        FormNama = "";
        FormTanggal = null;
        FormTarget = "";
        FormAktif = false;
        IsFormOpen = true;
    }

    private void EditRow(PeriodeKunjungan p)
    {
        if (!CanManage) return;
        _editId = p.PeriodeId;
        FormTitle = "Ubah periode";
        FormNama = p.Nama;
        FormTanggal = Fmt.ToDateTime(p.TanggalKunjungan);
        FormTarget = p.TargetPerusahaan.ToString();
        FormAktif = p.IsAktif;
        IsFormOpen = true;
    }

    [RelayCommand]
    private void CancelForm() => IsFormOpen = false;

    [RelayCommand]
    private async Task SaveFormAsync()
    {
        bool ok = await RunAsync(async () =>
        {
            if (!int.TryParse(FormTarget.Trim(), out int target)) throw new DomainException("Target harus berupa angka.");
            if (FormTanggal is null) throw new DomainException("Tanggal kunjungan wajib diisi.");
            await S.Master.SavePeriodeAsync(new PeriodeKunjungan
            {
                PeriodeId = _editId, Nama = FormNama, TanggalKunjungan = DateOnly.FromDateTime(FormTanggal.Value),
                TargetPerusahaan = target, IsAktif = FormAktif
            });
        }, "Periode disimpan.");
        if (ok)
        {
            IsFormOpen = false;
            await LoadAsync();
        }
    }
}
