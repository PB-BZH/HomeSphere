namespace HomeSphere.Wpf.ViewModels {
  class HistoryViewModel {
    private readonly Core.HomeSphere _homeSphere;
    public string Titre { get; } = "Historique";

    public HistoryViewModel(Core.HomeSphere homeSphere) {
      _homeSphere = homeSphere;
    }
  }
}
