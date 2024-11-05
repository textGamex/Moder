using CommunityToolkit.Mvvm.Messaging;
using MethodTimer;
using Moder.Core.Messages;
using NLog;

namespace Moder.Core.Services.GameResources;

// TODO: 可以考虑删了这个类
public sealed class GameResourcesService
{
    public StateCategoryService StateCategory => _stateCategoryLazy.Value;
    public LocalisationService Localisation => _localisationLazy.Value;
    public OreService OreService => _oreServiceLazy.Value;
    public BuildingsService Buildings => _buildingsLazy.Value;
    public CountryTagService CountryTagsService => _countryTagsLazy.Value;

    private readonly Lazy<StateCategoryService> _stateCategoryLazy;
    private Lazy<LocalisationService> _localisationLazy;
    private readonly Lazy<OreService> _oreServiceLazy;
    private readonly Lazy<BuildingsService> _buildingsLazy;
    private readonly Lazy<CountryTagService> _countryTagsLazy;
    private readonly GameResourcesWatcherService _watcherService;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public GameResourcesService(GameResourcesWatcherService watcherService)
    {
        _watcherService = watcherService;

        _stateCategoryLazy = new Lazy<StateCategoryService>(LoadStateCategory);
        _localisationLazy = new Lazy<LocalisationService>(LoadLocalisation);
        _oreServiceLazy = new Lazy<OreService>(LoadOre);
        _buildingsLazy = new Lazy<BuildingsService>(LoadBuildings);
        _countryTagsLazy = new Lazy<CountryTagService>(LoadCountriesTag);

        WeakReferenceMessenger.Default.Register<ReloadLocalizationFiles>(
            this,
            (_, _) => ReloadLocalisation()
        );
    }

    [Time("加载 Country Tags")]
    private CountryTagService LoadCountriesTag()
    {
        return new CountryTagService();
    }

    [Time("加载建筑物")]
    private BuildingsService LoadBuildings()
    {
        return new BuildingsService();
    }

    [Time("加载游戏内资源定义文件")]
    private OreService LoadOre()
    {
        return new OreService();
    }

    [Time("加载 StateCategoryService")]
    private StateCategoryService LoadStateCategory()
    {
        return new StateCategoryService();
    }

    [Time("加载本地化字符串")]
    private LocalisationService LoadLocalisation()
    {
        // TODO: 本地化暂时先不考虑 replace 文件夹
        return new LocalisationService();
    }

    private void ReloadLocalisation()
    {
        // 取消原文件夹的观察
        _watcherService.Unwatch(_localisationLazy.Value.FolderRelativePath);
        _localisationLazy = new Lazy<LocalisationService>(LoadLocalisation);

        Log.Info("重新加载本地化资源");
    }
}
