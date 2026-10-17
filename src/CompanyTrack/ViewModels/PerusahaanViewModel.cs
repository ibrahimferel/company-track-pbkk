using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;

namespace CompanyTrack.ViewModels;

public partial class PerusahaanRow : ObservableObject
{
    public Perusahaan Item { get; }
    public string Nama => Item.Nama;
    public string Industri => Item.BidangIndustri;
    public string Kota => Item.Kota ?? "-";
    public string Kontak => Item.NamaPIC is null ? "-" : $"{Item.NamaPIC} ({Item.JabatanPIC ?? "-"})";
    public string Email => Item.Email ?? "-";
    public bool CanWrite { get; }

    [ObservableProperty] private bool _isConfirmingDelete;

    public IRelayCommand EditCommand { get; }
    public IRelayCommand AskDeleteCommand { get; }
    public IRelayCommand CancelDeleteCommand { get; }
    public IRelayCommand ConfirmDeleteCommand { get; }

    public PerusahaanRow(Perusahaan item, bool canWrite, Action<PerusahaanRow> edit, Action<PerusahaanRow> delete)
    {
        Item = item;
        CanWrite = canWrite;
        EditCommand = new RelayCommand(() => edit(this));
        AskDeleteCommand = new RelayCommand(() => IsConfirmingDelete = true);
        CancelDeleteCommand = new RelayCommand(() => IsConfirmingDelete = false);
        ConfirmDeleteCommand = new RelayCommand(() => delete(this));
    }
}

public partial class PerusahaanViewModel : ViewModelBase
{
    private int _editId;

    public PerusahaanViewModel(AppServices services) : base(services) { }

    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private List<PerusahaanRow> _rows = new();

    [ObservableProperty] private bool _isFormOpen;
    [ObservableProperty] private string _formTitle = "";
    [ObservableProperty] private string _formNama = "";
    [ObservableProperty] private string _formIndustri = "";
    [ObservableProperty] private string _formKota = "";
    [ObservableProperty] private string _formEmail = "";
    [ObservableProperty] private string _formTelepon = "";
    [ObservableProperty] private string _formPic = "";
    [ObservableProperty] private string _formJabatan = "";

    public bool IsEmpty => Rows.Count == 0;
    partial void OnRowsChanged(List<PerusahaanRow> value) => OnPropertyChanged(nameof(IsEmpty));
    partial void OnSearchTextChanged(string value) => _ = LoadAsync();

    public override async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var list = await S.Master.ListPerusahaanAsync(SearchText);
            Rows = list.Select(p => new PerusahaanRow(p, CanWrite, EditRow, DeleteRowAsync)).ToList();
        });
    }

    [RelayCommand]
    private void NewPerusahaan()
    {
        if (!CanWrite) return;
        _editId = 0;
        FormTitle = "Tambah perusahaan";
        FormNama = FormIndustri = FormKota = FormEmail = FormTelepon = FormPic = FormJabatan = "";
        IsFormOpen = true;
    }

    private void EditRow(PerusahaanRow row)
    {
        if (!CanWrite) return;
        var p = row.Item;
        _editId = p.PerusahaanId;
        FormTitle = "Ubah perusahaan";
        FormNama = p.Nama;
        FormIndustri = p.BidangIndustri;
        FormKota = p.Kota ?? "";
        FormEmail = p.Email ?? "";
        FormTelepon = p.Telepon ?? "";
        FormPic = p.NamaPIC ?? "";
        FormJabatan = p.JabatanPIC ?? "";
        IsFormOpen = true;
    }

    [RelayCommand]
    private void CancelForm() => IsFormOpen = false;

    [RelayCommand]
    private async Task SaveFormAsync()
    {
        bool ok = await RunAsync(() => S.Master.SavePerusahaanAsync(new Perusahaan
        {
            PerusahaanId = _editId, Nama = FormNama, BidangIndustri = FormIndustri, Kota = FormKota,
            Email = FormEmail, Telepon = FormTelepon, NamaPIC = FormPic, JabatanPIC = FormJabatan
        }), "Perusahaan disimpan.");
        if (ok)
        {
            IsFormOpen = false;
            await LoadAsync();
        }
    }

    private async void DeleteRowAsync(PerusahaanRow row)
    {
        bool ok = await RunAsync(() => S.Master.DeletePerusahaanAsync(row.Item), "Perusahaan dihapus.");
        if (ok) await LoadAsync();
        else row.IsConfirmingDelete = false;
    }
}
