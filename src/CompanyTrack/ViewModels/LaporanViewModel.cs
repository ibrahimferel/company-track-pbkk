using CommunityToolkit.Mvvm.ComponentModel;
using CompanyTrack.Core.Models;

namespace CompanyTrack.ViewModels;

public class IndustriRow
{
    public string Industri { get; init; } = "";
    public int Total { get; init; }
    public int Diterima { get; init; }
    public int Ditolak { get; init; }
    public string Persen { get; init; } = "";
}

public class AlasanRow
{
    public string Alasan { get; init; } = "";
    public int Jumlah { get; init; }
    public double Max { get; init; }
}

public partial class LaporanViewModel : ViewModelBase
{
    private bool _loading;

    public LaporanViewModel(AppServices services) : base(services) { }

    public List<PeriodeKunjungan> PeriodeOptions { get; private set; } = new();
    [ObservableProperty] private PeriodeKunjungan? _selectedPeriode;
    [ObservableProperty] private string _summary = "";
    [ObservableProperty] private List<FunnelRow> _funnel = new();
    [ObservableProperty] private List<IndustriRow> _industri = new();
    [ObservableProperty] private List<AlasanRow> _alasan = new();

    partial void OnSelectedPeriodeChanged(PeriodeKunjungan? value) => _ = ReloadAsync();

    public override async Task LoadAsync()
    {
        _loading = true;
        await RunAsync(async () =>
        {
            PeriodeOptions = await S.Master.ListPeriodeAsync();
            OnPropertyChanged(nameof(PeriodeOptions));
            SelectedPeriode = PeriodeOptions.FirstOrDefault(p => p.IsAktif) ?? PeriodeOptions.FirstOrDefault();
        });
        _loading = false;
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        if (_loading || SelectedPeriode is null) return;
        var periode = SelectedPeriode;
        await RunAsync(async () =>
        {
            var r = await S.Dashboard.ReportAsync(periode.PeriodeId, periode.TargetPerusahaan);
            Summary = $"{r.Total} pengajuan, {r.Confirmed} terkonfirmasi dari target {r.Target}";
            double max = Math.Max(1, r.Funnel.Count == 0 ? 1 : r.Funnel.Max(f => f.Jumlah));
            Funnel = r.Funnel.Select(f => new FunnelRow { Label = f.Status.ToLabel(), Jumlah = f.Jumlah, Max = max }).ToList();
            Industri = r.Industri.Select(i => new IndustriRow
            {
                Industri = i.Industri, Total = i.Total, Diterima = i.Diterima, Ditolak = i.Ditolak,
                Persen = i.Total == 0 ? "-" : (i.Diterima * 100 / i.Total) + "%"
            }).ToList();
            double amax = Math.Max(1, r.Alasan.Count == 0 ? 1 : r.Alasan.Max(a => a.Jumlah));
            Alasan = r.Alasan.Select(a => new AlasanRow { Alasan = a.Alasan, Jumlah = a.Jumlah, Max = amax }).ToList();
        });
    }
}
