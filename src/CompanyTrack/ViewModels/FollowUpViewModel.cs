using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Models;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public class FollowUpRow
{
    public string Perusahaan { get; init; } = "";
    public string StatusLabel { get; init; } = "";
    public IBrush StatusBg { get; init; } = Palette.Gray;
    public IBrush StatusFg { get; init; } = Palette.GrayFg;
    public string Ringkasan { get; init; } = "";
    public string DueLabel { get; init; } = "";
    public IBrush DueBg { get; init; } = Palette.Gray;
    public IBrush DueFg { get; init; } = Palette.GrayFg;
    public string DateText { get; init; } = "";
    public FollowUpBucket Bucket { get; init; }
    public bool CanWrite { get; init; }
    public IRelayCommand DoneCommand { get; init; } = null!;
    public IRelayCommand OpenCommand { get; init; } = null!;
}

public partial class FollowUpViewModel : ViewModelBase
{
    private readonly INavigator _nav;
    private List<FollowUpRow> _all = new();

    public FollowUpViewModel(AppServices services, INavigator nav) : base(services) => _nav = nav;

    [ObservableProperty] private int _tabIndex;
    [ObservableProperty] private List<FollowUpRow> _rows = new();

    public string TabAll => $"Semua ({_all.Count})";
    public string TabLate => $"Terlambat ({_all.Count(r => r.Bucket == FollowUpBucket.Terlambat)})";
    public string TabToday => $"Hari ini ({_all.Count(r => r.Bucket == FollowUpBucket.HariIni)})";
    public string TabWeek => $"Minggu ini ({_all.Count(r => r.Bucket == FollowUpBucket.MingguIni)})";
    public bool IsEmpty => Rows.Count == 0;

    public bool IsTab0 => TabIndex == 0;
    public bool IsTab1 => TabIndex == 1;
    public bool IsTab2 => TabIndex == 2;
    public bool IsTab3 => TabIndex == 3;

    [RelayCommand]
    private void SelectTab(string index) => TabIndex = int.Parse(index);

    partial void OnTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsTab0));
        OnPropertyChanged(nameof(IsTab1));
        OnPropertyChanged(nameof(IsTab2));
        OnPropertyChanged(nameof(IsTab3));
        ApplyFilter();
    }
    partial void OnRowsChanged(List<FollowUpRow> value) => OnPropertyChanged(nameof(IsEmpty));

    private void ApplyFilter()
    {
        Rows = TabIndex switch
        {
            1 => _all.Where(r => r.Bucket == FollowUpBucket.Terlambat).ToList(),
            2 => _all.Where(r => r.Bucket == FollowUpBucket.HariIni).ToList(),
            3 => _all.Where(r => r.Bucket == FollowUpBucket.MingguIni).ToList(),
            _ => _all
        };
    }

    public override async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            var items = await S.Pengajuan.ListFollowUpsAsync();
            var today = DateOnly.FromDateTime(DateTime.Now);
            _all = items.Select(f =>
            {
                var bucket = FollowUpClassifier.Classify(f.TanggalFollowUp, today);
                var (sbg, sfg) = Palette.ForStatus(f.Status);
                var (dbg, dfg) = Palette.ForBucket(bucket);
                var item = f;
                return new FollowUpRow
                {
                    Perusahaan = f.Perusahaan, StatusLabel = f.Status.ToLabel(), StatusBg = sbg, StatusFg = sfg,
                    Ringkasan = f.Ringkasan, DueLabel = FollowUpClassifier.DueLabel(f.TanggalFollowUp, today),
                    DueBg = dbg, DueFg = dfg, DateText = $"{Fmt.Date(f.TanggalFollowUp)}, PIC {f.PicUsername}",
                    Bucket = bucket, CanWrite = CanWrite,
                    DoneCommand = new AsyncRelayCommand(() => MarkDoneAsync(item)),
                    OpenCommand = new AsyncRelayCommand(() => _nav.OpenDetailAsync(item.PengajuanId))
                };
            }).ToList();
            OnPropertyChanged(nameof(TabAll));
            OnPropertyChanged(nameof(TabLate));
            OnPropertyChanged(nameof(TabToday));
            OnPropertyChanged(nameof(TabWeek));
            ApplyFilter();
        });
    }

    private async Task MarkDoneAsync(FollowUpItem item)
    {
        bool ok = await RunAsync(() => S.Pengajuan.MarkFollowUpDoneAsync(item), "Follow-up ditandai selesai.");
        if (ok)
        {
            await LoadAsync();
            await _nav.RefreshBadgeAsync();
        }
    }
}
