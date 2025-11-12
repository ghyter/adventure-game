using AdventureGame.Editor.Services;

namespace AdventureGame
{
    public partial class App : Application
    {
        private readonly IThemePersistence _themePersistence;
        private readonly CurrentGameService _currentGameService;

        public App(IThemePersistence themePersistence, CurrentGameService currentGameService)
        {
            InitializeComponent();
            _themePersistence = themePersistence;
            _currentGameService = currentGameService;
        }

        protected override async void OnStart()
        {
            // This runs after MAUI and DI are ready.
            await _themePersistence.InitializeAsync();
            await _themePersistence.PersistOnChangeAsync();

            await _currentGameService.InitializeAsync();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "AdventureGame" };
        }
    }
}
