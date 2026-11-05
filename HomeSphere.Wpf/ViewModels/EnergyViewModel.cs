namespace HomeSphere.Wpf.ViewModels {
  class EnergyViewModel {
    private readonly Core.HomeSphere _homeSphere;
    public string Titre { get; } = "Energie";

    public EnergyViewModel(Core.HomeSphere homeSphere) {
      _homeSphere = homeSphere;
    }
  }
}
