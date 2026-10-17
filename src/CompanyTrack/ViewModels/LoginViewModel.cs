using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CompanyTrack.Core.Rules;

namespace CompanyTrack.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly Action _onSuccess;

    public LoginViewModel(AppServices services, Action onSuccess) : base(services) => _onSuccess = onSuccess;

    [ObservableProperty] private string _username = "";
    [ObservableProperty] private string _password = "";
    [ObservableProperty] private string _errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = "";
        try
        {
            IsBusy = true;
            await S.Auth.LoginAsync(Username, Password);
            Password = "";
            _onSuccess();
        }
        catch (DomainException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Tidak dapat terhubung ke database. Pastikan SQL Server (Docker) berjalan.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
