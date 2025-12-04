// Decompiled with JetBrains decompiler
// Type: Royal.Scenes.Game.Levels.Units.Explode.ExplodeTargetFinder
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6E3F9EDC-F65C-42C7-9F80-B7D0A788CB5D
// Assembly location: C:\Users\lilithgames\Desktop\royal_match\DummyDll\Assembly-CSharp.dll

using Il2CppDummyDll;
using Royal.Infrastructure.Contexts;
using Royal.Player.Context.Units;
using Royal.Scenes.Game.Context;
using Royal.Scenes.Game.Mechanics.Board.Cell;
using Royal.Scenes.Game.Mechanics.Board.Grid.Iterator;
using Royal.Scenes.Game.Mechanics.Explode;
using Royal.Scenes.Game.Mechanics.Matches;
using System.Collections.Generic;

#nullable disable
namespace Royal.Scenes.Game.Levels.Units.Explode
{
  [Token(Token = "0x2001C63")]
  public class ExplodeTargetFinder : IGameContextUnit, IContextUnit
  {
    [Token(Token = "0x400B08E")]
    [FieldOffset(Offset = "0x0")]
    private static int PropellerSortingOffset;
    [Token(Token = "0x400B08F")]
    [FieldOffset(Offset = "0x10")]
    private readonly List<int> rocketScoreIndexList;
    [Token(Token = "0x400B090")]
    [FieldOffset(Offset = "0x18")]
    private readonly List<CellPoint> areaScorePointList;
    [Token(Token = "0x400B091")]
    [FieldOffset(Offset = "0x20")]
    private CellGridManager gridManager;
    [Token(Token = "0x400B092")]
    [FieldOffset(Offset = "0x28")]
    private LevelRandomManager randomManager;
    [Token(Token = "0x400B093")]
    [FieldOffset(Offset = "0x30")]
    private GoalManager goalManager;
    [Token(Token = "0x400B094")]
    [FieldOffset(Offset = "0x38")]
    private LevelManager levelManager;
    [Token(Token = "0x400B095")]
    [FieldOffset(Offset = "0x40")]
    private GridIterator iterator;
    [Token(Token = "0x400B096")]
    [FieldOffset(Offset = "0x58")]
    private readonly SortedDictionary<int, List<CellModel>> scores;
    [Token(Token = "0x400B097")]
    [FieldOffset(Offset = "0x60")]
    private WinCalculationData winCalculationData;
    [Token(Token = "0x400B098")]
    [FieldOffset(Offset = "0x68")]
    private WaterWinCalculationData waterWinCalculationData;
    [Token(Token = "0x400B099")]
    [FieldOffset(Offset = "0x70")]
    private readonly WinCalculationData originalWinCalculationData;
    [Token(Token = "0x400B09A")]
    [FieldOffset(Offset = "0x78")]
    private readonly List<int> winningColumnsOrRows;
    [Token(Token = "0x400B09B")]
    [FieldOffset(Offset = "0x80")]
    private int[] columnScores;
    [Token(Token = "0x400B09C")]
    [FieldOffset(Offset = "0x88")]
    private int[] rowScores;
    [Token(Token = "0x400B09D")]
    [FieldOffset(Offset = "0x90")]
    private int[] goalDependentCounts;
    [Token(Token = "0x400B09E")]
    [FieldOffset(Offset = "0x98")]
    private bool[] hasAddedPositiveColumnScore;
    [Token(Token = "0x400B09F")]
    [FieldOffset(Offset = "0xA0")]
    private bool[] hasAddedPositiveRowScore;
    [Token(Token = "0x400B0A0")]
    [FieldOffset(Offset = "0xA8")]
    private bool isWaterLevel;
    [Token(Token = "0x400B0A1")]
    [FieldOffset(Offset = "0xB0")]
    private readonly bool[] clearedCellsAfterExploderHit;

    [Token(Token = "0x600C649")]
    [Address(RVA = "0x11A37F8", Offset = "0x11A37F8", VA = "0x11A37F8")]
    public ExplodeTargetFinder()
    {
    }

    [Token(Token = "0x600C64A")]
    [Address(RVA = "0x11A3C84", Offset = "0x11A3C84", VA = "0x11A3C84", Slot = "6")]
    public void Bind()
    {
    }

    [Token(Token = "0x600C64B")]
    [Address(RVA = "0x11A3EDC", Offset = "0x11A3EDC", VA = "0x11A3EDC")]
    private void OnCellGridViewsInitialized()
    {
    }

    [Token(Token = "0x600C64C")]
    [Address(RVA = "0x11A40B8", Offset = "0x11A40B8", VA = "0x11A40B8")]
    private void ArrangeWinCalculationData()
    {
    }

    [Token(Token = "0x600C64D")]
    [Address(RVA = "0x11A43F4", Offset = "0x11A43F4", VA = "0x11A43F4")]
    public void UpdateWaterWinCalculationData()
    {
    }

    [Token(Token = "0x170013C1")]
    public int Id
    {
      [Token(Token = "0x600C64E"), Address(RVA = "0x11A4488", Offset = "0x11A4488", VA = "0x11A4488", Slot = "5")] get
      {
        return new int();
      }
    }

    [Token(Token = "0x600C64F")]
    [Address(RVA = "0x11A4490", Offset = "0x11A4490", VA = "0x11A4490", Slot = "4")]
    public void Clear(bool gameExit)
    {
    }

    [Token(Token = "0x600C650")]
    [Address(RVA = "0x11A3E90", Offset = "0x11A3E90", VA = "0x11A3E90")]
    private static void ResetSortingOffset()
    {
    }

    [Token(Token = "0x600C651")]
    [Address(RVA = "0x11A472C", Offset = "0x11A472C", VA = "0x11A472C")]
    public static int GetNextSortingOffset(bool isExtraCombo) => new int();

    [Token(Token = "0x600C652")]
    [Address(RVA = "0x11A47BC", Offset = "0x11A47BC", VA = "0x11A47BC")]
    public void FindForExploder(GuidedExploderItemModel exploder)
    {
    }

    [Token(Token = "0x600C653")]
    [Address(RVA = "0x11A4BD4", Offset = "0x11A4BD4", VA = "0x11A4BD4")]
    private IExplodeTarget FindAreaTarget(ExplodeData exploder, bool canExploderSpreadJelly)
    {
      return (IExplodeTarget) null;
    }

    [Token(Token = "0x600C654")]
    [Address(RVA = "0x11A7768", Offset = "0x11A7768", VA = "0x11A7768")]
    private CellModel GetPrioritizedCellInColumnForChainBottomCollect(
      GuidedExploderType guidedExploderType,
      Cell14 cells)
    {
      return (CellModel) null;
    }

    [Token(Token = "0x600C655")]
    [Address(RVA = "0x11A7A70", Offset = "0x11A7A70", VA = "0x11A7A70")]
    private CellModel GetPrioritizedCellForJellySpread(bool canExploderSpreadJelly, Cell14 cells)
    {
      return (CellModel) null;
    }

    [Token(Token = "0x600C656")]
    [Address(RVA = "0x11A7C84", Offset = "0x11A7C84", VA = "0x11A7C84")]
    public int GetColumnScore(
      int column,
      bool canExploderSpreadJelly,
      out bool hasAddedPositiveScoreItem)
    {
      return new int();
    }

    [Token(Token = "0x600C657")]
    [Address(RVA = "0x11A8194", Offset = "0x11A8194", VA = "0x11A8194")]
    public int GetRowScore(
      int row,
      bool canExploderSpreadJelly,
      out bool hasAddedPositiveScoreItem)
    {
      return new int();
    }

    [Token(Token = "0x600C658")]
    [Address(RVA = "0x11A8018", Offset = "0x11A8018", VA = "0x11A8018")]
    private int GetCellScoreForRocket(
      CellModel cell,
      GuidedExploderType guidedExploderType,
      bool canExploderSpreadJelly)
    {
      return new int();
    }

    [Token(Token = "0x600C659")]
    [Address(RVA = "0x11A7008", Offset = "0x11A7008", VA = "0x11A7008")]
    private IExplodeTarget FindSingleTarget(GuidedExploderItemModel guidedExploder)
    {
      return (IExplodeTarget) null;
    }

    [Token(Token = "0x600C65A")]
    [Address(RVA = "0x11A8B40", Offset = "0x11A8B40", VA = "0x11A8B40")]
    public CellModel FindSingleTargetForCellExploder(ExplodeData explodeData) => (CellModel) null;

    [Token(Token = "0x600C65B")]
    [Address(RVA = "0x11A86D8", Offset = "0x11A86D8", VA = "0x11A86D8")]
    private (bool, CellModel) FindSingleTargetCellForExploder(
      ExplodeData explodeData,
      IExplodeTarget targetItem)
    {
      return ();
    }

    [Token(Token = "0x600C65C")]
    [Address(RVA = "0x11A8BB0", Offset = "0x11A8BB0", VA = "0x11A8BB0")]
    private int FindHighestScoreForRocket(int[] scores) => new int();

    [Token(Token = "0x600C65D")]
    [Address(RVA = "0x11A4CD4", Offset = "0x11A4CD4", VA = "0x11A4CD4")]
    private void FillScores(GuidedExploderType guidedExploderType, bool canExploderSpreadJelly)
    {
    }

    [Token(Token = "0x600C65E")]
    [Address(RVA = "0x11A8D78", Offset = "0x11A8D78", VA = "0x11A8D78")]
    private bool ShouldAddToSingleTargetScore(CellModel cell) => new bool();

    [Token(Token = "0x600C65F")]
    [Address(RVA = "0x11A850C", Offset = "0x11A850C", VA = "0x11A850C")]
    private bool ShouldAddToTotalScore(CellModel cell) => new bool();

    [Token(Token = "0x600C660")]
    [Address(RVA = "0x11A9018", Offset = "0x11A9018", VA = "0x11A9018")]
    private bool ShouldAddDueToBelowItem(CellModel cell) => new bool();

    [Token(Token = "0x600C661")]
    [Address(RVA = "0x11A4A18", Offset = "0x11A4A18", VA = "0x11A4A18")]
    private IExplodeTarget FindPowerCubeOrSoilTargetForWinCondition(ExplodeData exploder)
    {
      return (IExplodeTarget) null;
    }

    [Token(Token = "0x600C662")]
    [Address(RVA = "0x11A4508", Offset = "0x11A4508", VA = "0x11A4508")]
    private void ClearLists(bool gameExit = false)
    {
    }

    [Token(Token = "0x600C663")]
    [Address(RVA = "0x11A8D5C", Offset = "0x11A8D5C", VA = "0x11A8D5C")]
    private void ClearGoalDependentItems()
    {
    }

    [Token(Token = "0x600C664")]
    [Address(RVA = "0x11A6010", Offset = "0x11A6010", VA = "0x11A6010")]
    private IExplodeTarget FindColumnTarget(
      ExplodeData exploder,
      GuidedExploderType guidedExploderType,
      bool canExploderSpreadJelly,
      bool isForDebugDisplay = false)
    {
      return (IExplodeTarget) null;
    }

    [Token(Token = "0x600C665")]
    [Address(RVA = "0x11A510C", Offset = "0x11A510C", VA = "0x11A510C")]
    private IExplodeTarget FindRowTarget(
      ExplodeData exploder,
      GuidedExploderType guidedExploderType,
      bool canExploderSpreadJelly,
      bool isForDebugDisplay = false)
    {
      return (IExplodeTarget) null;
    }

    [Token(Token = "0x600C666")]
    [Address(RVA = "0x11A709C", Offset = "0x11A709C", VA = "0x11A709C")]
    public CellPoint FindHighestScoreForAreaExploder(
      ExplodeData exploder,
      bool canExploderSpreadJelly,
      bool isForDebugDisplay = false,
      IComparer<CellPoint> customComparer = null)
    {
      return new CellPoint();
    }

    [Token(Token = "0x600C667")]
    [Address(RVA = "0x11AA628", Offset = "0x11AA628", VA = "0x11AA628")]
    private CellPoint GetRandomItemFromAreaScores(IComparer<CellPoint> customComparer)
    {
      return new CellPoint();
    }

    [Token(Token = "0x600C668")]
    [Address(RVA = "0x11A914C", Offset = "0x11A914C", VA = "0x11A914C")]
    public int CalculateScoreForAreaExploder(
      CellPoint point,
      bool canExploderSpreadJelly,
      Trigger trigger,
      out bool hasAddedPositiveScoreItem)
    {
      return new int();
    }

    [Token(Token = "0x600C669")]
    [Address(RVA = "0x11A8080", Offset = "0x11A8080", VA = "0x11A8080")]
    private bool HasTargetableBelowItemUnderLightball(CellModel cell) => new bool();

    [Token(Token = "0x600C66A")]
    [Address(RVA = "0x11AA700", Offset = "0x11AA700", VA = "0x11AA700")]
    private void ResetPerformanceData()
    {
    }

    [Token(Token = "0x600C66B")]
    [Address(RVA = "0x11A4A14", Offset = "0x11A4A14", VA = "0x11A4A14")]
    private void BeginProfile()
    {
    }

    [Token(Token = "0x600C66C")]
    [Address(RVA = "0x11A4CD0", Offset = "0x11A4CD0", VA = "0x11A4CD0")]
    private void EndProfile()
    {
    }

    [Token(Token = "0x600C66D")]
    [Address(RVA = "0x11AA704", Offset = "0x11AA704", VA = "0x11AA704")]
    public void AddExploderMetrics(List<object> metrics)
    {
    }
  }
}
