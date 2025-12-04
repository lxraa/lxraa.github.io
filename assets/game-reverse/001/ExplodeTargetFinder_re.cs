using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Il2CppDummyDll;
using Royal.Infrastructure.Contexts;
using Royal.Player.Context.Units;
using Royal.Scenes.Game.Context;
using Royal.Scenes.Game.Mechanics.Board.Cell;
using Royal.Scenes.Game.Mechanics.Board.Grid.Iterator;
using Royal.Scenes.Game.Mechanics.Explode;
using Royal.Scenes.Game.Mechanics.Matches;

namespace Royal.Scenes.Game.Levels.Units.Explode
{
	// Token: 0x02002014 RID: 8212
	[Token(Token = "0x2002014")]
	public class ExplodeTargetFinder : IGameContextUnit, IContextUnit
	{
		// Token: 0x0600E03A RID: 57402 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E03A")]
		[Address(RVA = "0x1ABF388", Offset = "0x1ABF388", VA = "0x1ABF388")]
		public ExplodeTargetFinder()
		{
			// 初始化胜利列/行列表
			winningColumnsOrRows = new List<int>();
			
			// 初始化爆炸后清除的单元格标记数组，大小为99
			clearedCellsAfterExploderHit = new bool[99];
			
			// 调用基类构造函数
			// base();
			
			// 初始化分数字典
			scores = new SortedDictionary<int, List<CellModel>>();
			
			// 初始化胜利计算数据
			winCalculationData = new WinCalculationData();
			
			// 将原始胜利计算数据设置为当前胜利计算数据的引用
			originalWinCalculationData = winCalculationData;
			
			// 初始化火箭分数索引列表，容量为11
			rocketScoreIndexList = new List<int>(11);
			
			// 初始化区域分数点列表，容量为35
			areaScorePointList = new List<CellPoint>(35);
		}

		// Token: 0x0600E03B RID: 57403 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E03B")]
		[Address(RVA = "0x1ABF814", Offset = "0x1ABF814", VA = "0x1ABF814", Slot = "6")]
		public void Bind()
		{
			// 获取CellGridManager (LevelContextId.CellGridManager = 2)
			gridManager = Royal.Scenes.Game.Levels.LevelContext.Get<Royal.Scenes.Game.Levels.Units.CellGridManager>(Royal.Scenes.Game.Levels.LevelContextId.CellGridManager);
			
			// 获取LevelRandomManager (LevelContextId.LevelRandomManager = 0)
			randomManager = Royal.Scenes.Game.Levels.LevelContext.Get<Royal.Scenes.Game.Levels.Units.LevelRandomManager>(Royal.Scenes.Game.Levels.LevelContextId.LevelRandomManager);
			
			// 获取GoalManager (LevelContextId.GoalManager = 12)
			goalManager = Royal.Scenes.Game.Levels.LevelContext.Get<Royal.Scenes.Game.Levels.Units.GoalManager>(Royal.Scenes.Game.Levels.LevelContextId.GoalManager);
			
			// 获取LevelManager (UserContextId.LevelManager = 2)
			levelManager = Royal.Player.Context.UserContext.Get<Royal.Player.Context.Units.LevelManager>(Royal.Player.Context.UserContextId.LevelManager);
			
			// 设置isWaterLevel字段，从LevelManager的静态字段获取
			isWaterLevel = Royal.Player.Context.Units.LevelManager.IsWaterLevel;
			
			// 创建Action并订阅CellGridManager的OnCellGridViewsInitialized事件
			if (gridManager != null)
			{
				var action = new System.Action(OnCellGridViewsInitialized);
				gridManager.add_OnCellGridViewsInitialized(action);
			}
			
			// 重置PropellerSortingOffset静态字段为10000
			PropellerSortingOffset = 10000;
		}

		// Token: 0x0600E03C RID: 57404 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E03C")]
		[Address(RVA = "0x1ABFA6C", Offset = "0x1ABFA6C", VA = "0x1ABFA6C")]
		private void OnCellGridViewsInitialized()
		{
			// 安排胜利计算数据
			ArrangeWinCalculationData();
			
			// 检查网格管理器是否存在
			if (gridManager == null)
				return;
			
			// 创建列分数数组
			int width = gridManager.Width;
			columnScores = new int[width];
			
			// 创建行分数数组
			int height = gridManager.Height;
			rowScores = new int[height];
			
			// 创建正分数列标记数组
			hasAddedPositiveColumnScore = new bool[width];
			
			// 创建正分数行标记数组
			hasAddedPositiveRowScore = new bool[height];
			
			// 初始化网格迭代器
			iterator = gridManager.GetIterator(true);
			
			// 检查目标管理器并创建目标依赖计数数组
			if (goalManager != null)
			{
				var currentGoals = goalManager.GetCurrentGoals();
				if (currentGoals != null && currentGoals.Length > 0)
				{
					// 检查是否有任何目标是依赖于目标的（hasGoalDependency字段为true）
					bool hasGoalDependentTarget = false;
					for (int i = 0; i < currentGoals.Length; i++)
					{
						var goal = currentGoals[i];
						if (goal != null && goal.hasGoalDependency)
						{
							hasGoalDependentTarget = true;
							break;
						}
					}
					
					// 如果有依赖目标的项，创建目标依赖计数数组
					if (hasGoalDependentTarget)
					{
						goalDependentCounts = new int[currentGoals.Length];
					}
				}
			}
		}

		// Token: 0x0600E03D RID: 57405 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E03D")]
		[Address(RVA = "0x1ABFC48", Offset = "0x1ABFC48", VA = "0x1ABFC48")]
		private void ArrangeWinCalculationData()
		{
			// 检查当前winCalculationData是否是WaterWinCalculationData类型
			bool isCurrentlyWaterData = winCalculationData is WaterWinCalculationData;
			
			if (isCurrentlyWaterData)
			{
				// 如果当前是WaterWinCalculationData但不是水关，恢复原始数据
				if (!isWaterLevel)
				{
					winCalculationData = originalWinCalculationData;
					waterWinCalculationData = null;
				}
			}
			else if (isWaterLevel)
			{
				// 如果是水关但当前不是WaterWinCalculationData，创建WaterWinCalculationData
				if (waterWinCalculationData == null)
				{
					waterWinCalculationData = new WaterWinCalculationData();
				}
				winCalculationData = waterWinCalculationData;
			}
		}

		// Token: 0x0600E03E RID: 57406 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E03E")]
		[Address(RVA = "0x1ABFF84", Offset = "0x1ABFF84", VA = "0x1ABFF84")]
		public void UpdateWaterWinCalculationData()
		{
			// 检查waterWinCalculationData是否存在
			if (waterWinCalculationData != null)
			{
				// 调用waterWinCalculationData的OnLevelStart方法
				waterWinCalculationData.OnLevelStart();
			}
		}

		// Token: 0x17001621 RID: 5665
		// (get) Token: 0x0600E03F RID: 57407 RVA: 0x00048360 File Offset: 0x00046560
		[Token(Token = "0x17001621")]
		public int Id
		{
			[Token(Token = "0x600E03F")]
			[Address(RVA = "0x1AC0018", Offset = "0x1AC0018", VA = "0x1AC0018", Slot = "5")]
			get
			{
				return 13;
			}
		}

		// Token: 0x0600E040 RID: 57408 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E040")]
		[Address(RVA = "0x1AC0020", Offset = "0x1AC0020", VA = "0x1AC0020", Slot = "4")]
		public void Clear(bool gameExit)
		{
			// 清空列表
			ClearLists(gameExit);
			
			// 如果是游戏退出，清空目标依赖计数数组
			if (gameExit)
			{
				goalDependentCounts = null;
			}
			
			// 重置MatchItemExplodeScoreCalculator
			Royal.Scenes.Game.Mechanics.Items.MatchItem.MatchItemExplodeScoreCalculator.Reset();
			
			// 重置PropellerSortingOffset为10000
			PropellerSortingOffset = 10000;
		}

		// Token: 0x0600E041 RID: 57409 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E041")]
		[Address(RVA = "0x1ABFA20", Offset = "0x1ABFA20", VA = "0x1ABFA20")]
		private static void ResetSortingOffset()
		{
			PropellerSortingOffset = 10000;
		}

		// Token: 0x0600E042 RID: 57410 RVA: 0x00048378 File Offset: 0x00046578
		[Token(Token = "0x600E042")]
		[Address(RVA = "0x1AC02BC", Offset = "0x1AC02BC", VA = "0x1AC02BC")]
		public static int GetNextSortingOffset(bool isExtraCombo)
		{
			// 将PropellerSortingOffset减1
			PropellerSortingOffset--;
			
			// 如果PropellerSortingOffset小于0，重置为10000
			if (PropellerSortingOffset < 0)
			{
				PropellerSortingOffset = 10000;
			}
			
			// 如果是额外组合，返回PropellerSortingOffset - 22
			// 否则返回PropellerSortingOffset
			if (isExtraCombo)
			{
				return PropellerSortingOffset - 22;
			}
			else
			{
				return PropellerSortingOffset;
			}
		}

		// Token: 0x0600E043 RID: 57411 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E043")]
		[Address(RVA = "0x1AC034C", Offset = "0x1AC034C", VA = "0x1AC034C")]
		public void FindForExploder(GuidedExploderItemModel exploder)
		{
			if (exploder == null)
				return;
				
			// 提取爆炸器的目标爆炸数据
			var explodeData = exploder.TargetExplodeData;
			var trigger = explodeData.trigger;
			var canSpreadJelly = explodeData.canSpreadJelly;
			
			// 将trigger转换为GuidedExploderType
			var guidedExploderType = TriggerExtensions.AsGuidedExploderType(trigger);
			
			// 检查winCalculationData是否存在
			if (winCalculationData == null)
				return;
			
			// 调用winCalculationData的MapAreas方法进行区域映射
			winCalculationData.MapAreas();
			
			IExplodeTarget target = null;
			
			// 根据不同的trigger类型执行不同的查找策略
			switch (trigger)
			{
				case Trigger.TntPropeller: // 13
					// 先尝试找PowerCube或Soil目标用于胜利条件
					target = FindPowerCubeOrSoilTargetForWinCondition(explodeData);
					if (target == null)
					{
						// 没找到就尝试找区域目标
						target = FindAreaTarget(explodeData, canSpreadJelly);
						if (target == null)
						{
							// 最后尝试找单一目标
							ClearLists(false);
							FillScores(guidedExploderType, canSpreadJelly);
							target = FindSingleTarget(exploder);
						}
					}
					break;
					
				case Trigger.VerticalPropellerRocket: // 10
					// 清空列表并填充分数
					ClearLists(false);
					FillScores(guidedExploderType, canSpreadJelly);
					
					// 先尝试找PowerCube或Soil目标用于胜利条件
					target = FindPowerCubeOrSoilTargetForWinCondition(explodeData);
					if (target == null)
					{
						// 没找到就尝试找列目标
						target = FindColumnTarget(explodeData, guidedExploderType, canSpreadJelly, false);
						if (target == null)
						{
							// 最后尝试找单一目标
							target = FindSingleTarget(exploder);
						}
					}
					break;
					
				case Trigger.HorizontalPropellerRocket: // 11
					// 清空列表并填充分数
					ClearLists(false);
					FillScores(guidedExploderType, canSpreadJelly);
					
					// 先尝试找PowerCube或Soil目标用于胜利条件
					target = FindPowerCubeOrSoilTargetForWinCondition(explodeData);
					if (target == null)
					{
						// 没找到就尝试找行目标
						target = FindRowTarget(explodeData, guidedExploderType, canSpreadJelly, false);
						if (target == null)
						{
							// 最后尝试找单一目标
							target = FindSingleTarget(exploder);
						}
					}
					break;
					
				default:
					// 其他情况直接找单一目标
					ClearLists(false);
					FillScores(guidedExploderType, canSpreadJelly);
					target = FindSingleTarget(exploder);
					break;
			}
			
			// 调用exploder的TargetFound方法通知找到目标
			exploder.TargetFound(target);
		}

		// Token: 0x0600E044 RID: 57412 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E044")]
		[Address(RVA = "0x1AC07D0", Offset = "0x1AC07D0", VA = "0x1AC07D0")]
		private IExplodeTarget FindAreaTarget(ExplodeData exploder, bool canExploderSpreadJelly)
		{
			// 调用FindHighestScoreForAreaExploder获取最高分数的位置
			var targetPoint = FindHighestScoreForAreaExploder(exploder, canExploderSpreadJelly, false, null);
			
			// 检查gridManager是否存在
			if (gridManager == null)
				return null;
			
			// 通过gridManager获取目标位置的单元格
			var targetCell = gridManager[targetPoint];
			if (targetCell == null)
				return null;
			
			// 检查单元格的ExplodeTargetMediator是否存在
			if (targetCell.ExplodeTargetMediator == null)
				return null;
			
			// 通过ExplodeTargetMediator添加传入的爆炸器并返回目标
			return targetCell.ExplodeTargetMediator.AddIncomingExploder(exploder);
		}

		// Token: 0x0600E045 RID: 57413 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E045")]
		[Address(RVA = "0x1AC3650", Offset = "0x1AC3650", VA = "0x1AC3650")]
		private CellModel GetPrioritizedCellInColumnForChainBottomCollect(GuidedExploderType guidedExploderType, Cell14 cells)
		{
			// 检查是否有底部收集关卡
			if (levelManager == null || !levelManager.IsThereBottomCollectInLevel())
				return null;
			
			// 检查关卡中是否有Chain道具 (StaticItemType.Chain = 6)
			if (!levelManager.IsThereItemInLevel(StaticItemType.Chain))
				return null;
			
			// 创建候选单元格列表
			var candidateCells = new Cell14();
			
			bool hasChainItem = false;
			bool hasBoxItem = false;
			
			// 遍历所有传入的单元格
			for (int i = 0; i < cells.Count; i++)
			{
				var cell = cells[i];
				if (cell == null) continue;
				
				var currentItem = cell.CurrentItem;
				
				// 检查是否有Box道具 (ItemType.Box = 16)
				if (currentItem != null && currentItem.ItemType == ItemType.Box)
				{
					hasBoxItem = true;
				}
				
				// 检查是否有Frog道具 (ItemType.Frog = 31)
				if (currentItem != null && currentItem.ItemType == ItemType.Frog)
				{
					hasBoxItem = true;
				}
				
				// 检查是否有RoyalBot道具 (ItemType.RoyalBot = 96)
				if (currentItem != null && currentItem.ItemType == ItemType.RoyalBot)
				{
					hasBoxItem = true;
				}
				
				// 检查是否有Chain静态道具
				if (cell.StaticMediator?.HasStaticItem(StaticItemType.Chain) == true)
				{
					hasChainItem = true;
				}
				
				// 检查是否有爆炸目标中介器且分数 >= 1
				if (cell.ExplodeTargetMediator != null)
				{
					int score = cell.ExplodeTargetMediator.GetScore(guidedExploderType, false, null);
					if (score >= 1)
					{
						// 检查是否有下方道具或上方道具
						bool hasBelowItem = cell.StaticMediator?.HasBelowItem() == true;
						bool hasAboveItem = cell.StaticMediator?.HasAboveItem() == true;
						
						if (hasBelowItem || hasAboveItem)
						{
							candidateCells.Add(cell);
						}
						else if (cell.CurrentItem != null)
						{
							// 检查当前道具是否是特殊道具
							if (!cell.CurrentItem.IsSpecialItem())
							{
								candidateCells.Add(cell);
							}
						}
					}
				}
			}
			
			// 如果同时有Chain道具、Box道具，且候选列表不为空
			if (hasChainItem && hasBoxItem && candidateCells.Count > 0)
			{
				// 使用随机管理器打乱候选列表并返回第一个
				if (randomManager != null)
				{
					var shuffledCells = new Cell14();
					candidateCells.CopyTo(shuffledCells);
					randomManager.ShuffleCells(shuffledCells);
					return shuffledCells[0];
				}
			}
			
			return null;
		}

		// Token: 0x0600E046 RID: 57414 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E046")]
		[Address(RVA = "0x1AC39A0", Offset = "0x1AC39A0", VA = "0x1AC39A0")]
		private CellModel GetPrioritizedCellForJellySpread(bool canExploderSpreadJelly, Cell14 cells)
		{
			// 检查关卡管理器是否存在
			if (levelManager == null)
				return null;
			
			// 检查关卡中是否有Jelly道具 (StaticItemType.Jelly = 3)
			if (!levelManager.IsThereItemInLevel(StaticItemType.Jelly))
				return null;
			
			// 创建候选单元格列表
			var candidateCells = new Cell14();
			
			bool hasJellyReceiver = false;
			
			// 遍历所有传入的单元格
			for (int i = 0; i < cells.Count; i++)
			{
				var cell = cells[i];
				if (cell == null)
					continue;
				
				// 检查单元格是否能接收果冻
				if (cell.CanReceiveJelly(false))
				{
					hasJellyReceiver = true;
				}
				
				if (canExploderSpreadJelly)
				{
					// 如果爆炸器能传播果冻，检查单元格是否为普通单元格且没有果冻传播阻挡道具
					if (cell.IsNormalCell() && !cell.HasJellySpreadBlockingItem(false))
					{
						candidateCells.Add(cell);
					}
				}
				else
				{
					// 如果爆炸器不能传播果冻，检查单元格是否能传播果冻
					if (cell.CanSpreadJelly(false))
					{
						candidateCells.Add(cell);
					}
				}
			}
			
			// 如果有果冻接收器且候选列表不为空
			if (hasJellyReceiver && candidateCells.Count > 0)
			{
				// 使用随机管理器打乱候选列表并返回第一个
				if (randomManager != null)
				{
					var shuffledCells = new Cell14();
					candidateCells.CopyTo(shuffledCells);
					randomManager.ShuffleCells(shuffledCells);
					return shuffledCells[0];
				}
			}
			
			return null;
		}

		// Token: 0x0600E047 RID: 57415 RVA: 0x00048390 File Offset: 0x00046590
		[Token(Token = "0x600E047")]
		[Address(RVA = "0x1AC3BB4", Offset = "0x1AC3BB4", VA = "0x1AC3BB4")]
		public int GetColumnScore(int column, bool canExploderSpreadJelly, out bool hasAddedPositiveScoreItem)
		{
			hasAddedPositiveScoreItem = false;
			
			// 清空爆炸后清除的单元格标记数组
			if (clearedCellsAfterExploderHit != null)
			{
				System.Array.Clear(clearedCellsAfterExploderHit, 0, clearedCellsAfterExploderHit.Length);
			}
			
			if (gridManager == null)
				return 0;
				
			int boardStartY = gridManager.BoardStartY;
			int boardEndY = gridManager.BoardEndY;
			
			int totalScore = 0;
			int treasureMapHitCount = 0; // v35 - 宝藏地图击中次数
			int logHitCount = 0; // v20 - 日志击中次数
			
			// 遍历列中的每个单元格
			for (int y = boardStartY; y <= boardEndY; y++)
			{
				var cellPoint = new CellPoint(column, y);
				var cell = gridManager[cellPoint];
				
				// 获取单元格的火箭分数
				int cellScore = GetCellScoreForRocket(cell, GuidedExploderType.VerticalRocket, canExploderSpreadJelly);
				
				// 检查是否添加了正分数项
				bool hasPositiveScore = cellScore > 0;
				if (!hasPositiveScore)
				{
					hasPositiveScore = HasTargetableBelowItemUnderLightball(cell);
				}
				hasAddedPositiveScoreItem = hasAddedPositiveScoreItem || hasPositiveScore;
				
				if (cell?.StaticMediator == null)
					continue;
					
				totalScore += cellScore;
				
				// 如果没有上方道具，检查当前道具
				if (!cell.StaticMediator.HasAboveItem())
				{
					if (cell.Mediator?.HasCurrentItem() == true)
					{
						var currentItem = cell.CurrentItem;
						if (currentItem != null)
						{
							var itemType = currentItem.ItemType;
							
							// 检查不同类型的道具
							if (itemType == ItemType.Lightball) // 5
							{
								// 如果是灯泡且分数大于0，减去特定值
								if (cellScore >= 1)
								{
									totalScore -= 1599395; // 0xFFE7985D的补码
								}
							}
							else if (itemType == ItemType.MapScrollCollect) // 48
							{
								// 检查宝藏地图剩余爆炸次数
								if (currentItem.RemainingExplodeCount() <= 1)
								{
									treasureMapHitCount++;
								}
							}
							else if (itemType == ItemType.LogCollect) // 80
							{
								// 检查日志剩余爆炸次数
								if (currentItem.RemainingExplodeCount() < 2)
								{
									logHitCount++;
								}
							}
							
							// 如果是底部收集关卡，更新清除单元格数据
							if (levelManager?.IsThereBottomCollectInLevel() == true)
							{
								currentItem.UpdateClearedCellPointsAfterExploderHit(
									GuidedExploderType.VerticalRocket,
									clearedCellsAfterExploderHit,
									new CellPoint(column, 0));
							}
						}
					}
				}
			}
			
			// 如果是底部收集关卡，添加额外分数
			if (levelManager?.IsThereBottomCollectInLevel() == true)
			{
				int additionalScore = gridManager.GetBottomCollectVerticalRocketScoreForClearedCells(
					clearedCellsAfterExploderHit,
					new CellPoint(column, 0));
				totalScore += additionalScore;
			}
			
			// 添加宝藏地图额外分数
			if (treasureMapHitCount >= 1)
			{
				var treasureMapHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.TreasureMapItem.TreasureMapHelper>(Royal.Scenes.Game.Levels.LevelContextId.TreasureMapHelper);
				if (treasureMapHelper != null)
				{
					totalScore += treasureMapHelper.GetTreasureRevealExtraScore(treasureMapHitCount);
				}
			}
			
			// 添加日志额外分数
			if (logHitCount >= 1)
			{
				var logItemHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.LogItem.LogItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.LogItemHelper);
				if (logItemHelper != null)
				{
					totalScore += logItemHelper.GetLogActivateExtraScore(logHitCount);
				}
			}
			
			return totalScore;
		}

		// Token: 0x0600E048 RID: 57416 RVA: 0x000483A8 File Offset: 0x000465A8
		[Token(Token = "0x600E048")]
		[Address(RVA = "0x1AC40C4", Offset = "0x1AC40C4", VA = "0x1AC40C4")]
		public int GetRowScore(int row, bool canExploderSpreadJelly, out bool hasAddedPositiveScoreItem)
		{
			hasAddedPositiveScoreItem = false;
			
			// 清空爆炸后清除的单元格标记数组
			if (clearedCellsAfterExploderHit != null)
			{
				System.Array.Clear(clearedCellsAfterExploderHit, 0, clearedCellsAfterExploderHit.Length);
			}
			
			if (gridManager == null)
				return 0;
				
			int totalScore = 0;
			int treasureMapHitCount = 0; // 宝藏地图击中次数
			int logHitCount = 0; // 日志击中次数
			
			// 遍历行中的每个单元格
			for (int x = 0; x < gridManager.Width; x++)
			{
				var cellPoint = new CellPoint(x, row);
				var cell = gridManager[cellPoint];
				
				// 获取单元格的火箭分数
				int cellScore = GetCellScoreForRocket(cell, GuidedExploderType.HorizontalRocket, canExploderSpreadJelly);
				
				// 检查是否添加了正分数项
				bool hasPositiveScore = cellScore > 0;
				if (!hasPositiveScore)
				{
					hasPositiveScore = HasTargetableBelowItemUnderLightball(cell);
				}
				hasAddedPositiveScoreItem = hasAddedPositiveScoreItem || hasPositiveScore;
				
				if (cell?.StaticMediator == null)
					continue;
					
				totalScore += cellScore;
				
				// 如果没有上方道具，检查当前道具
				if (!cell.StaticMediator.HasAboveItem())
				{
					if (cell.Mediator?.HasCurrentItem() == true)
					{
						var currentItem = cell.CurrentItem;
						if (currentItem != null)
						{
							var itemType = currentItem.ItemType;
							
							// 检查不同类型的道具
							if (itemType == ItemType.Lightball) // 5
							{
								// 如果是灯泡且分数大于0，减去特定值
								if (cellScore >= 1)
								{
									totalScore -= 1599395; // 0xFFE7985D的补码
								}
							}
							else if (itemType == ItemType.MapScrollCollect) // 48
							{
								// 检查宝藏地图剩余爆炸次数
								if (currentItem.RemainingExplodeCount() <= 1)
								{
									treasureMapHitCount++;
								}
							}
							else if (itemType == ItemType.LogCollect) // 80
							{
								// 检查日志剩余爆炸次数
								if (currentItem.RemainingExplodeCount() < 2)
								{
									logHitCount++;
								}
							}
							
							// 如果是底部收集关卡，更新清除单元格数据
							if (levelManager?.IsThereBottomCollectInLevel() == true)
							{
								currentItem.UpdateClearedCellPointsAfterExploderHit(
									GuidedExploderType.HorizontalRocket,
									clearedCellsAfterExploderHit,
									new CellPoint(0, row));
							}
						}
					}
				}
			}
			
			// 如果是底部收集关卡，添加额外分数
			if (levelManager?.IsThereBottomCollectInLevel() == true)
			{
				int additionalScore = gridManager.GetBottomCollectHorizontalRocketScoreForClearedCells(
					clearedCellsAfterExploderHit,
					new CellPoint(0, row));
				totalScore += additionalScore;
			}
			
			// 添加宝藏地图额外分数
			if (treasureMapHitCount >= 1)
			{
				var treasureMapHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.TreasureMapItem.TreasureMapHelper>(Royal.Scenes.Game.Levels.LevelContextId.TreasureMapHelper);
				if (treasureMapHelper != null)
				{
					totalScore += treasureMapHelper.GetTreasureRevealExtraScore(treasureMapHitCount);
				}
			}
			
			// 添加日志额外分数
			if (logHitCount >= 1)
			{
				var logItemHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.LogItem.LogItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.LogItemHelper);
				if (logItemHelper != null)
				{
					totalScore += logItemHelper.GetLogActivateExtraScore(logHitCount);
				}
			}
			
			return totalScore;
		}

		// Token: 0x0600E049 RID: 57417 RVA: 0x000483C0 File Offset: 0x000465C0
		[Token(Token = "0x600E049")]
		[Address(RVA = "0x1AC3F48", Offset = "0x1AC3F48", VA = "0x1AC3F48")]
		private int GetCellScoreForRocket(CellModel cell, GuidedExploderType guidedExploderType, bool canExploderSpreadJelly)
		{
			// 如果单元格为空，返回0
			if (cell == null)
				return 0;
			
			// 获取爆炸目标中介器
			var explodeTargetMediator = cell.ExplodeTargetMediator;
			if (explodeTargetMediator == null)
				return 0;
			
			// 获取基础分数
			int score = explodeTargetMediator.GetScore(guidedExploderType, canExploderSpreadJelly, null);
			
			// 如果分数为0，直接返回0
			if (score == 0)
				return 0;
			
			// 检查是否应该添加到总分数
			if (ShouldAddToTotalScore(cell))
			{
				return score;
			}
			
			// 如果不应该添加到总分数，返回0
			return 0;
		}

		// Token: 0x0600E04A RID: 57418 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E04A")]
		[Address(RVA = "0x1AC2DB4", Offset = "0x1AC2DB4", VA = "0x1AC2DB4")]
		private IExplodeTarget FindSingleTarget(GuidedExploderItemModel guidedExploder)
		{
			// 检查guidedExploder是否为null
			if (guidedExploder == null)
				return null;
			
			// 获取爆炸数据
			var explodeData = guidedExploder.TargetExplodeData;
			
			// 调用FindSingleTargetCellForExploder获取目标单元格
			var result = FindSingleTargetCellForExploder(explodeData, guidedExploder.TargetItem);
			
			// 如果第一个值为true，说明找到了现有的目标道具
			if (result.Item1)
			{
				return guidedExploder.TargetItem;
			}
			
			// 如果第二个值不为null，说明找到了新的目标单元格
			if (result.Item2 != null)
			{
				// 通过单元格的ExplodeTargetMediator创建目标
				if (result.Item2.ExplodeTargetMediator != null)
				{
					return result.Item2.ExplodeTargetMediator.AddIncomingExploder(explodeData);
				}
			}
			
			// 没有找到合适的目标
			return null;
		}

		// Token: 0x0600E04B RID: 57419 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E04B")]
		[Address(RVA = "0x1AC4ACC", Offset = "0x1AC4ACC", VA = "0x1AC4ACC")]
		public CellModel FindSingleTargetForCellExploder(ExplodeData explodeData)
		{
			// 清空列表
			ClearLists(false);
			
			// 将trigger转换为GuidedExploderType
			var guidedExploderType = TriggerExtensions.AsGuidedExploderType(explodeData.trigger);
			
			// 填充分数
			FillScores(guidedExploderType, explodeData.canSpreadJelly);
			
			// 调用FindSingleTargetCellForExploder并返回第二个元素（CellModel）
			var result = FindSingleTargetCellForExploder(explodeData, null);
			return result.Item2;
		}

		// Token: 0x0600E04C RID: 57420 RVA: 0x000483D8 File Offset: 0x000465D8
		[Token(Token = "0x600E04C")]
		[Address(RVA = "0x1AC4608", Offset = "0x1AC4608", VA = "0x1AC4608")]
		private ValueTuple<bool, CellModel> FindSingleTargetCellForExploder(ExplodeData explodeData, IExplodeTarget targetItem)
		{
			// 如果是回旋镖触发器（Trigger = 35），使用专门的回旋镖查找逻辑
			if (explodeData.trigger == (Trigger)35)
			{
				return FindSingleTargetCellForBoomerang(explodeData, targetItem);
			}
			
			// 检查分数字典是否存在
			if (scores == null)
			{
				Royal.Infrastructure.Services.Logs.Log.Debug(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, 
					"No valid exploder target found", new object[0]);
				return new ValueTuple<bool, CellModel>(false, null);
			}
			
			// 遍历所有分数等级（从高分到低分）
			foreach (var scoreEntry in scores)
			{
				var cellList = scoreEntry.Value;
				
				// 特殊处理：如果有目标道具且分数为0，返回true表示找到目标但没有具体单元格
				if (targetItem != null && scoreEntry.Key == 0)
				{
					return new ValueTuple<bool, CellModel>(true, null);
				}
				
				if (cellList == null)
					continue;
				
				// 持续尝试从当前分数等级的列表中找到合适的目标
				while (cellList.Count > 0)
				{
					// 随机选择一个单元格索引
					if (randomManager == null)
						break;
						
					int randomIndex = randomManager.Next(0, cellList.Count);
					var selectedCell = cellList[randomIndex];
					
					if (selectedCell == null)
						break;
					
					// 检查是否与爆炸数据的起始点相同，如果相同且列表有多个元素，选择不同的单元格
					if (selectedCell.Point.Equals(explodeData.point) && cellList.Count >= 2)
					{
						// 选择不同的索引
						randomIndex = randomIndex == 0 ? 1 : randomIndex - 1;
						selectedCell = cellList[randomIndex];
						
						if (selectedCell == null)
							break;
					}
					
					// 检查目标单元格的剩余爆炸次数
					if (selectedCell.ExplodeTargetMediator != null)
					{
						int remainingCount = selectedCell.ExplodeTargetMediator.RemainingExplodeCount(explodeData);
						
						// 如果剩余次数不足，从列表中移除该单元格并继续尝试
						if (remainingCount <= 1)
						{
							cellList.RemoveAt(randomIndex);
							
							// 如果剩余次数小于1，跳过这个单元格
							if (remainingCount < 1)
								continue;
						}
						
						// 如果剩余次数足够，尝试重定向目标
						var redirectedCell = selectedCell.ExplodeTargetMediator.TryRedirect(explodeData, cellList);
						return new ValueTuple<bool, CellModel>(true, redirectedCell);
					}
				}
			}
			
			// 如果没有找到合适的目标，记录调试信息
			Royal.Infrastructure.Services.Logs.Log.Debug(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, 
				"No valid exploder target found", new object[0]);
			
			return new ValueTuple<bool, CellModel>(false, null);
		}

		// Token: 0x0600E04D RID: 57421 RVA: 0x000483F0 File Offset: 0x000465F0
		[Token(Token = "0x600E04D")]
		[Address(RVA = "0x1AC4B44", Offset = "0x1AC4B44", VA = "0x1AC4B44")]
		private ValueTuple<bool, CellModel> FindSingleTargetCellForBoomerang(ExplodeData explodeData, IExplodeTarget targetItem)
		{
			if (scores == null)
				return new ValueTuple<bool, CellModel>(false, null);
			
			CellModel foundTargetCell = null;
			Royal.Scenes.Game.Mechanics.Board.Grid.Sorter.BoomerangItemCellSorter boomerangSorter = null;
			
			// 遍历所有分数等级（从高分到低分）
			foreach (var scoreEntry in scores)
			{
				var cellList = scoreEntry.Value;
				if (cellList == null || cellList.Count == 0)
					continue;
				
				// 获取回旋镖排序器
				if (Royal.Scenes.Game.Mechanics.Board.Grid.Sorter.CellSorter.BoomerangItemCellSorter != null)
				{
					boomerangSorter = Royal.Scenes.Game.Mechanics.Board.Grid.Sorter.CellSorter.BoomerangItemCellSorter;
				}
				
				// 设置起始单元格点
				if (targetItem != null)
				{
					// 如果有目标道具，尝试获取其位置作为起始点
					var targetPosition = targetItem.GetPosition();
					if (targetPosition.HasValue && boomerangSorter != null)
					{
						boomerangSorter.startCellPoint = targetPosition.Value;
						foundTargetCell = targetItem as CellModel;
					}
					else if (boomerangSorter != null)
					{
						boomerangSorter.startCellPoint = explodeData.point;
					}
				}
				else if (boomerangSorter != null)
				{
					boomerangSorter.startCellPoint = explodeData.point;
				}
				
				// 设置反向传入方向
				if (boomerangSorter != null)
				{
					boomerangSorter.reversedIncomingDirection = explodeData.incomingDirection;
				}
				
				// 打乱单元格列表顺序
				if (randomManager != null)
				{
					Royal.Infrastructure.Utils.Extensions.ListExtensions.Shuffle(cellList, randomManager);
				}
				
				// 如果起始点不是默认值，初始化PatternData
				if (boomerangSorter != null && 
					!boomerangSorter.startCellPoint.Equals(Royal.Scenes.Game.Mechanics.Board.Cell.CellPoint.Default))
				{
					foreach (var cell in cellList)
					{
						if (cell?.ExplodeTargetMediator != null)
						{
							cell.ExplodeTargetMediator.InitRandom(randomManager);
						}
					}
					
					// 使用回旋镖排序器对列表排序
					cellList.Sort(boomerangSorter);
				}
				
				// 特殊处理：如果是特定的目标道具且分数为0
				if (targetItem != null && scoreEntry.Key == 0)
				{
					// 检查是否有可击中的目标
					if (foundTargetCell != null && foundTargetCell.IsHittingUseful())
					{
						return new ValueTuple<bool, CellModel>(true, foundTargetCell);
					}
					
					// 遍历单元格列表寻找合适的目标
					for (int i = 0; i < cellList.Count; i++)
					{
						var cell = cellList[i];
						if (cell?.CurrentItem != null && cell.CurrentItem.IsSpecialItem())
						{
							foundTargetCell = cell;
							break;
						}
					}
					
					return new ValueTuple<bool, CellModel>(foundTargetCell == null, foundTargetCell);
				}
				
				// 正常情况下寻找目标
				if (cellList.Count > 0)
				{
					int bestIndex = 0;
					
					// 寻找最佳目标索引
					for (int i = 0; i < cellList.Count; i++)
					{
						var cell = cellList[i];
						if (cell == null) continue;
						
						// 检查是否与目标道具标识符匹配
						var cellIdentifier = cell.GetIdentifierOfItemTakingDamage();
						if (cellIdentifier.Equals(explodeData.lastTargetItemIdentifier))
						{
							bestIndex = i;
							break;
						}
						
						// 检查剩余爆炸次数
						if (cell.ExplodeTargetMediator != null)
						{
							int remainingCount = cell.ExplodeTargetMediator.RemainingExplodeCount(explodeData);
							if (remainingCount >= 1)
							{
								bestIndex = i;
								break;
							}
						}
					}
					
					// 获取最佳目标单元格
					var targetCell = cellList[bestIndex];
					if (targetCell?.ExplodeTargetMediator != null)
					{
						int remainingCount = targetCell.ExplodeTargetMediator.RemainingExplodeCount(explodeData);
						if (remainingCount >= 1)
						{
							// 尝试重定向
							var redirectedCell = targetCell.ExplodeTargetMediator.TryRedirect(explodeData, cellList);
							
							// 检查爆炸器索引是否在有效范围内（1-2）
							if (explodeData.exploderIndex >= 1 && explodeData.exploderIndex <= 2)
							{
								return new ValueTuple<bool, CellModel>(true, redirectedCell);
							}
						}
					}
				}
			}
			
			// 如果没有找到合适的目标，记录调试信息
			Royal.Infrastructure.Services.Logs.Log.Debug(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, 
				"No valid boomerang target found", new object[0]);
			
			return new ValueTuple<bool, CellModel>(false, null);
		}

		// Token: 0x0600E04E RID: 57422 RVA: 0x00048408 File Offset: 0x00046608
		[Token(Token = "0x600E04E")]
		[Address(RVA = "0x1AC58AC", Offset = "0x1AC58AC", VA = "0x1AC58AC")]
		private int FindHighestScoreForRocket(int[] scores)
		{
			// 清空火箭分数索引列表
			if (rocketScoreIndexList != null)
			{
				rocketScoreIndexList.Clear();
			}
			
			// 如果分数数组为空，返回0
			if (scores == null || scores.Length == 0)
				return 0;
			
			int highestScoreIndex = 0;
			int highestScore = scores[0];
			
			// 遍历分数数组找到最高分数
			for (int i = 0; i < scores.Length; i++)
			{
				int currentScore = scores[i];
				
				if (currentScore == highestScore)
				{
					// 如果分数相等，添加到候选列表
					if (rocketScoreIndexList != null)
					{
						rocketScoreIndexList.Add(i);
					}
				}
				else if (currentScore > highestScore)
				{
					// 如果找到更高的分数，清空列表并添加新的索引
					highestScore = currentScore;
					highestScoreIndex = i;
					
					if (rocketScoreIndexList != null)
					{
						rocketScoreIndexList.Clear();
						rocketScoreIndexList.Add(i);
					}
				}
			}
			
			// 如果有多个相同的最高分数，随机选择一个
			if (randomManager != null && rocketScoreIndexList != null && rocketScoreIndexList.Count > 0)
			{
				return randomManager.GetRandomItemFromList(rocketScoreIndexList);
			}
			
			return highestScoreIndex;
		}

		// Token: 0x0600E04F RID: 57423 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E04F")]
		[Address(RVA = "0x1AC0884", Offset = "0x1AC0884", VA = "0x1AC0884")]
		private void FillScores(GuidedExploderType guidedExploderType, bool canExploderSpreadJelly)
		{
			// 重置迭代器
			iterator.Reset();
			
			// 清空目标依赖计数数组
			if (goalDependentCounts != null)
			{
				System.Array.Clear(goalDependentCounts, 0, goalDependentCounts.Length);
			}
			
			// 遍历所有单元格
			while (iterator.MoveNext())
			{
				// 清空爆炸后清除的单元格标记数组
				if (clearedCellsAfterExploderHit != null)
				{
					System.Array.Clear(clearedCellsAfterExploderHit, 0, clearedCellsAfterExploderHit.Length);
				}
				
				CellModel cell = iterator.Cell;
				if (cell == null)
					continue;
					
				// 只处理普通单元格
				if (!cell.IsNormalCell())
					continue;
					
				// 检查是否有螺旋桨阻挡道具
				if (cell.StaticMediator != null && cell.StaticMediator.HasPropellerBlockingItem())
					continue;
					
				// 获取爆炸目标中介器并计算分数
				if (cell.ExplodeTargetMediator == null)
					continue;
					
				int score = cell.ExplodeTargetMediator.GetScore(guidedExploderType, canExploderSpreadJelly, null);
				
				// 检查是否应该添加到单目标分数
				if (!ShouldAddToSingleTargetScore(cell))
					continue;
					
				int finalScore = -score; // 负分数用于排序（高分优先）
				
				// 特殊处理：底部收集关卡的单一爆炸器策略
				if (levelManager != null && 
					levelManager.IsThereBottomCollectInLevel() && 
					GameplayExtensions.IsSingleExploderStrategy(guidedExploderType))
				{
					// 检查是否有上方道具
					if (cell.StaticMediator != null && cell.StaticMediator.HasAboveItem())
					{
						var topMostAboveItem = cell.StaticMediator.GetTopMostAboveItem();
						if (topMostAboveItem != null && 
							topMostAboveItem is ChainItemModel && 
							topMostAboveItem.RemainingExplodeCount() <= 1)
						{
							goto CalculateBottomCollectScore;
						}
					}
					else
					{
						CalculateBottomCollectScore:
						// 检查当前单元格是否有道具
						if (cell.Mediator != null && cell.Mediator.HasCurrentItem())
						{
							var currentItem = cell.CurrentItem;
							if (currentItem != null)
							{
								bool hasAboveItem = cell.StaticMediator != null && cell.StaticMediator.HasAboveItem();
								
								if (!hasAboveItem)
								{
									// 调用UpdateClearedCellPointsAfterExploderHit方法模拟爆炸清除单元格
									currentItem.UpdateClearedCellPointsAfterExploderHit(
										(GuidedExploderType)0, // 这里IDA显示传入0
										clearedCellsAfterExploderHit, 
										cell.Point);
								}
								
								// 检查道具是否为特殊道具
								if (currentItem.IsSpecialItem() && currentItem is SpecialItemModel specialItem)
								{
									// 检查isPreparedForSpecialUse字段 (偏移0xB2)
									if (!specialItem.isPreparedForSpecialUse)
									{
										// 计算底部收集单一爆炸器的额外分数
										if (gridManager != null)
										{
											int additionalScore = gridManager.GetBottomCollectSingleExploderScoreForClearedCells(
												clearedCellsAfterExploderHit, cell.Point);
											finalScore = -(score + additionalScore);
										}
									}
								}
								else
								{
									// 计算底部收集单一爆炸器的额外分数
									if (gridManager != null)
									{
										int additionalScore = gridManager.GetBottomCollectSingleExploderScoreForClearedCells(
											clearedCellsAfterExploderHit, cell.Point);
										finalScore = -(score + additionalScore);
									}
								}
							}
						}
					}
				}
				
				// 将单元格添加到对应分数的列表中
				if (scores.TryGetValue(finalScore, out List<CellModel> cellList))
				{
					cellList.Add(cell);
				}
				else
				{
					var newList = new List<CellModel>();
					newList.Add(cell);
					scores.Add(finalScore, newList);
				}
			}
		}

		// Token: 0x0600E050 RID: 57424 RVA: 0x00048420 File Offset: 0x00046620
		[Token(Token = "0x600E050")]
		[Address(RVA = "0x1AC5A74", Offset = "0x1AC5A74", VA = "0x1AC5A74")]
		private bool ShouldAddToSingleTargetScore(CellModel cell)
		{
			// 如果是水关，直接返回true
			if (LevelManager.IsWaterLevel)
				return true;
			
			// 检查关卡中是否有ForceField道具 (StaticItemType.ForceField = 15)
			if (levelManager != null && levelManager.IsThereItemInLevel(StaticItemType.ForceField))
			{
				if (cell?.StaticMediator != null)
				{
					// 获取最上方的道具
					var topMostAboveItem = cell.StaticMediator.GetTopMostAboveItem();
					if (topMostAboveItem is Royal.Scenes.Game.Mechanics.Items.StaticItems.ForceFieldItem.ForceFieldItemModel forceFieldItem)
					{
						// 如果是leader，返回true
						if (forceFieldItem.isLeader)
							return true;
						
						// 否则检查剩余爆炸次数是否为1
						return forceFieldItem.BaseRemainingExplodeCount() == 1;
					}
				}
			}
			
			// 检查关卡中是否有Soil道具 (ItemType.Soil = 28)
			if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.Soil))
			{
				if (cell != null)
				{
					var currentItem = cell.CurrentItem;
					if (currentItem is Royal.Scenes.Game.Mechanics.Items.SoilItem.SoilItemModel soilItem)
					{
						// 如果是leader，返回true
						if (soilItem.IsLeader())
							return true;
						
						// 否则检查剩余爆炸次数是否为1
						return soilItem.BaseRemainingExplodeCount() == 1;
					}
				}
			}
			
			// 检查关卡中是否有Ivy道具 (ItemType.Ivy = 66)
			if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.Ivy))
			{
				if (cell != null)
				{
					var currentItem = cell.CurrentItem;
					if (currentItem is Royal.Scenes.Game.Mechanics.Items.IvyItem.IvyItemModel ivyItem)
					{
						// 如果是头部，返回true
						if (ivyItem.IsHead)
							return true;
						
						// 否则检查剩余爆炸次数是否为1
						return ivyItem.BaseRemainingExplodeCount() == 1;
					}
				}
			}
			
			// 检查关卡中是否有PowerCube道具 (ItemType.PowerCube = 85)
			if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.PowerCube))
			{
				if (cell != null)
				{
					var currentItem = cell.CurrentItem;
					if (currentItem is Royal.Scenes.Game.Mechanics.Items.PowerCubeItem.PowerCubeItemModel powerCubeItem)
					{
						// 如果是leader，返回true
						if (powerCubeItem.IsLeader())
							return true;
						
						// 否则检查剩余爆炸次数是否为1
						return powerCubeItem.BaseRemainingExplodeCount() == 1;
					}
				}
			}
			else
			{
				// 如果没有PowerCube道具，直接返回true
				return true;
			}
			
			return true;
		}

		// Token: 0x0600E051 RID: 57425 RVA: 0x00048438 File Offset: 0x00046638
		[Token(Token = "0x600E051")]
		[Address(RVA = "0x1AC443C", Offset = "0x1AC443C", VA = "0x1AC443C")]
		private bool ShouldAddToTotalScore(CellModel cell)
		{
			// 如果目标依赖计数数组为空，返回true
			if (goalDependentCounts == null)
				return true;
			
			if (cell == null)
				return true;
			
			// 检查单元格是否有中介器
			if (cell.Mediator == null)
				return true;
			
			// 获取当前道具
			var currentItem = cell.Mediator.CurrentItem;
			
			// 检查当前道具是否实现了IGoalDependedExplodeTarget接口
			if (!(currentItem is Royal.Scenes.Game.Levels.Units.Explode.IGoalDependedExplodeTarget goalDependedTarget))
				return true;
			
			// 检查静态中介器
			if (cell.StaticMediator == null)
				return true;
			
			// 如果有触摸阻挡道具，返回true
			if (cell.StaticMediator.HasTouchBlockingItem())
				return true;
			
			// 如果由于下方道具应该添加，返回true
			if (ShouldAddDueToBelowItem(cell))
				return true;
			
			// 获取目标类型
			var goalType = goalDependedTarget.GoalType;
			
			if (goalManager == null)
				return true;
			
			// 获取目标索引
			int goalIndex = goalManager.GetGoalIndex(goalType, 0);
			if (goalIndex < 0)
				return true;
			
			// 获取目标剩余数量
			int goalLeftCount = goalManager.GetGoalLeftCount(goalType, 0);
			
			if (goalDependentCounts == null)
				return true;
			
			// 检查数组边界
			if (goalIndex >= goalDependentCounts.Length)
				return true;
			
			// 检查当前目标的依赖计数
			int currentCount = goalDependentCounts[goalIndex];
			
			// 如果剩余目标数量大于当前计数，增加计数并返回true
			if (goalLeftCount > currentCount)
			{
				goalDependentCounts[goalIndex] = currentCount + 1;
				return true;
			}
			
			// 否则返回false，不应该添加到总分数中
			return false;
		}

		// Token: 0x0600E052 RID: 57426 RVA: 0x00048450 File Offset: 0x00046650
		[Token(Token = "0x600E052")]
		[Address(RVA = "0x1AC5D14", Offset = "0x1AC5D14", VA = "0x1AC5D14")]
		private bool ShouldAddDueToBelowItem(CellModel cell)
		{
			// 检查单元格是否为空
			if (cell == null)
				return false;
			
			// 检查静态中介器是否存在
			if (cell.StaticMediator == null)
				return false;
			
			// 检查是否有下方道具
			if (!cell.StaticMediator.HasBelowItem())
				return false;
			
			// 检查中介器是否存在
			if (cell.Mediator == null)
				return false;
			
			// 获取当前道具
			var currentItem = cell.Mediator.CurrentItem;
			if (currentItem == null)
				return false;
			
			// 获取道具类型
			var itemType = currentItem.ItemType;
			
			// 如果是Ufo道具，返回false
			if (itemType == ItemType.Ufo) // ItemType.Ufo = 45
				return false;
			
			// 如果不是CookieJar且不是ChocoMaker，返回true
			return itemType != ItemType.CookieJar && itemType != ItemType.ChocoMaker; // ItemType.CookieJar = 33, ItemType.ChocoMaker = 93
		}

		// Token: 0x0600E053 RID: 57427 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E053")]
		[Address(RVA = "0x1AC0604", Offset = "0x1AC0604", VA = "0x1AC0604")]
		private IExplodeTarget FindPowerCubeOrSoilTargetForWinCondition(ExplodeData exploder)
		{
			if (levelManager == null)
				return null;
				
			CellModel targetCell = null;
			
					// 首先检查关卡中是否有PowerCube道具
		if (levelManager.IsThereItemInLevel(ItemType.PowerCube)) // ItemType.PowerCube = 85
		{
			// 获取PowerCube助手
			var powerCubeHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PowerCubeItem.PowerCubeItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PowerCubeItemHelper);
			if (powerCubeHelper != null)
			{
				// 尝试获取只有一层的PowerCube位置
				var powerCubeCellPoint = powerCubeHelper.TryGetLastGroupsPowerCubeCellPointWithOneLayer();
				if (powerCubeCellPoint.HasValue && gridManager != null)
				{
					// 获取对应位置的单元格
					targetCell = gridManager[powerCubeCellPoint.Value];
				}
			}
		}
		
		// 如果没有找到PowerCube目标，检查是否有Soil道具
		if (targetCell == null && levelManager.IsThereItemInLevel(ItemType.Soil)) // ItemType.Soil = 28
		{
			// 获取Soil助手
			var soilHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.SoilItem.SoilItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.SoilItemHelper);
			if (soilHelper != null)
			{
				// 尝试获取只有一层的Soil位置
				var soilCellPoint = soilHelper.TryGetLastGroupsSoilCellPointWithOneLayer();
				if (soilCellPoint.HasValue && gridManager != null)
				{
					// 获取对应位置的单元格
					targetCell = gridManager[soilCellPoint.Value];
				}
			}
		}
			
			// 如果找到了目标单元格，返回其爆炸目标
			if (targetCell?.ExplodeTargetMediator != null)
			{
				return targetCell.ExplodeTargetMediator.AddIncomingExploder(exploder);
			}
			
			return null;
		}

		// Token: 0x0600E054 RID: 57428 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E054")]
		[Address(RVA = "0x1AC0098", Offset = "0x1AC0098", VA = "0x1AC0098")]
		private void ClearLists(bool gameExit = false)
		{
			// 清空分数字典
			if (scores != null)
			{
				if (gameExit)
				{
					// 游戏退出时直接清空整个字典
					scores.Clear();
				}
				else
				{
					// 正常情况下遍历字典中的每个列表并清空
					foreach (var kvp in scores)
					{
						var list = kvp.Value;
						if (list != null)
						{
							list.Clear();
						}
					}
				}
			}
			
			// 清空胜利计算数据
			if (winCalculationData != null)
			{
				winCalculationData.ClearGoalData();
			}
			
			// 清空目标依赖计数数组
			if (goalDependentCounts != null)
			{
				System.Array.Clear(goalDependentCounts, 0, goalDependentCounts.Length);
			}
			
			// 清空行分数数组
			if (rowScores != null)
			{
				System.Array.Clear(rowScores, 0, rowScores.Length);
			}
			
			// 清空列分数数组
			if (columnScores != null)
			{
				System.Array.Clear(columnScores, 0, columnScores.Length);
			}
		}

		// Token: 0x0600E055 RID: 57429 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E055")]
		[Address(RVA = "0x1AC5A58", Offset = "0x1AC5A58", VA = "0x1AC5A58")]
		private void ClearGoalDependentItems()
		{
			// 清空目标依赖计数数组
			if (goalDependentCounts != null)
			{
				System.Array.Clear(goalDependentCounts, 0, goalDependentCounts.Length);
			}
		}

		// Token: 0x0600E056 RID: 57430 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E056")]
		[Address(RVA = "0x1AC1CB4", Offset = "0x1AC1CB4", VA = "0x1AC1CB4")]
		private IExplodeTarget FindColumnTarget(ExplodeData exploder, GuidedExploderType guidedExploderType, bool canExploderSpreadJelly, bool isForDebugDisplay = false)
		{
			// 清空胜利列/行列表
			if (winningColumnsOrRows != null)
			{
				winningColumnsOrRows.Clear();
			}
			
			// 检查是否有ChocoMaker道具并更新额外分数
			if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.ChocoMaker)) // ItemType.ChocoMaker = 93
			{
				var chocoMakerHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.ChocoMakerItem.ChocoMakerItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ChocoMakerItemHelper);
				if (chocoMakerHelper != null && columnScores != null)
				{
					chocoMakerHelper.UpdateExtraScoresForChocoMakers(guidedExploderType, columnScores.Length);
				}
			}
			
			if (columnScores == null)
				return null;
				
			// 遍历所有列计算分数
			for (int column = 0; column < columnScores.Length; column++)
			{
				// 清空爆炸后清除的单元格标记数组
				if (clearedCellsAfterExploderHit != null)
				{
					System.Array.Clear(clearedCellsAfterExploderHit, 0, clearedCellsAfterExploderHit.Length);
				}
				
				// 计算列分数
				bool hasAddedPositiveScoreItem;
				int score = GetColumnScore(column, canExploderSpreadJelly, out hasAddedPositiveScoreItem);
				
				// 存储分数
				columnScores[column] = score;
				
				// 存储是否有正分数项
				if (hasAddedPositiveColumnScore != null)
				{
					hasAddedPositiveColumnScore[column] = hasAddedPositiveScoreItem;
				}
				
				// 检查是否应该添加到胜利列表
				if (winCalculationData != null && winCalculationData.CheckColumn(column, exploder))
				{
					// 记录到日志
					var logValues = new object[] { column };
					Royal.Infrastructure.Services.Logs.Log.Debug(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, "Column added to winning list", logValues);
					
					// 添加到胜利列表
					if (winningColumnsOrRows != null)
					{
						winningColumnsOrRows.Add(column);
					}
				}
				
				// 如果不能传播果冻，添加螺旋桨垂直火箭组合额外分数
				if (!exploder.canSpreadJelly)
				{
					int extraScore = Royal.Scenes.Game.Mechanics.Items.StaticItems.JellyItem.JellyItemModel.GetPropellerVerticalRocketComboExtraScore(column);
					columnScores[column] += extraScore;
				}
				
				// 各种道具的额外分数处理
				if (levelManager != null)
				{
					// IceCube道具处理
					if (guidedExploderType == GuidedExploderType.VerticalRocket && levelManager.IsThereItemInLevel(ItemType.IceCube)) // ItemType.IceCube = 51
					{
						var iceCubeHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.IceCubeItem.IceCubeItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.IceCubeItemHelper);
						if (iceCubeHelper != null)
						{
							int iceCubeCount = iceCubeHelper.GetHittableIceCubeCountInColumn(column, GuidedExploderType.VerticalRocket);
							if (iceCubeCount >= 2)
							{
								int explodeScore = iceCubeHelper.ExplodeScore();
								columnScores[column] += explodeScore * (iceCubeCount - 1);
							}
						}
					}
					
					// IceCrusher道具处理
					if (levelManager.IsThereItemInLevel(ItemType.IceCrusher)) // ItemType.IceCrusher = 19
					{
						var iceCrusherHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.IceCrusherItem.IceCrusherItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.IceCrusherItemHelper);
						if (iceCrusherHelper != null)
						{
							int scoreReduction = iceCrusherHelper.GetVerticalLogFakeItemScoreToBeReduced(column, isWaterLevel);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// MetalCrusher道具处理
					if (levelManager.IsThereItemInLevel(ItemType.MetalCrusher)) // ItemType.MetalCrusher = 32
					{
						var metalCrusherHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.MetalCrusherItem.MetalCrusherItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.MetalCrusherItemHelper);
						if (metalCrusherHelper != null)
						{
							int scoreReduction = metalCrusherHelper.GetVerticalLogFakeItemScoreToBeReduced(column, isWaterLevel);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// Soil道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Soil)) // ItemType.Soil = 28
					{
						var soilHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.SoilItem.SoilItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.SoilItemHelper);
						if (soilHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = soilHelper.GetSoilScoreOffsetForPropellerCombo(GuidedExploderType.VerticalRocket, column, defaultPoint, isWaterLevel);
							columnScores[column] += extraScore;
						}
					}
					
					// Magnet道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Magnet)) // ItemType.Magnet = 56
					{
						var magnetHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.MagnetItem.MagnetItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.MagnetItemHelper);
						if (magnetHelper != null)
						{
							var defaultPoint = new CellPoint();
							int scoreReduction = magnetHelper.GetMultipleHeadScoreToBeReduced(column, guidedExploderType, defaultPoint, isWaterLevel);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// CandyCane道具处理
					if (levelManager.IsThereItemInLevel(ItemType.CandyCane)) // ItemType.CandyCane = 57
					{
						var candyCaneHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CandyCaneItem.CandyCaneItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.CandyCaneItemHelper);
						if (candyCaneHelper != null)
						{
							int scoreReduction = candyCaneHelper.GetScoreDifferenceForRocketCombo(column, false);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// Coil道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Coil)) // ItemType.Coil = 61
					{
						var coilHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CoilItem.CoilItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.CoilItemHelper);
						if (coilHelper != null)
						{
							int scoreReduction = coilHelper.GetExtraIncludedRocketScore(false, column);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// CookieJar道具处理
					if (levelManager.IsThereItemInLevel(ItemType.CookieJar)) // ItemType.CookieJar = 33
					{
						var cookieJarHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CookieJarItem.View.CookieJarCollectHelper>(Royal.Scenes.Game.Levels.LevelContextId.CookieJarCollectHelper);
						if (cookieJarHelper != null)
						{
							int scoreReduction = cookieJarHelper.GetExtraIncludedRocketScore(false, column);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// Ufo道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Ufo)) // ItemType.Ufo = 45
					{
						var ufoHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.UfoItem.View.UfoCollectHelper>(Royal.Scenes.Game.Levels.LevelContextId.UfoCollectHelper);
						if (ufoHelper != null)
						{
							int scoreReduction = ufoHelper.GetExtraIncludedRocketScore(false, column);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// Ivy道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Ivy)) // ItemType.Ivy = 66
					{
						var ivyHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.IvyItem.IvyItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.IvyItemHelper);
						if (ivyHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = ivyHelper.GetIvyScoreOffsetForPropellerCombo(GuidedExploderType.VerticalRocket, column, defaultPoint, isWaterLevel);
							columnScores[column] += extraScore;
						}
					}
					
					// Blinds道具处理
					if (levelManager.IsThereItemInLevel(StaticItemType.Blinds)) // StaticItemType.Blinds = 11
					{
						var blindsHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.BlindsItem.BlindsItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.BlindsItemHelper);
						if (blindsHelper != null)
						{
							int scoreReduction = blindsHelper.GetVerticalScoreToBeReduced(column, isWaterLevel);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// Porcelain道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Porcelain)) // ItemType.Porcelain = 70
					{
						var porcelainHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PorcelainItem.PorcelainItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PorcelainItemHelper);
						if (porcelainHelper != null)
						{
							int scoreReduction = porcelainHelper.GetVerticalPorcelainFakeItemScoreToBeReduced(column, isWaterLevel);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// AncientVault道具处理
					if (levelManager.IsThereItemInLevel(ItemType.AncientVault)) // ItemType.AncientVault = 75
					{
						var ancientVaultHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.AncientVaultItem.AncientVaultItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.AncientVaultItemHelper);
						if (ancientVaultHelper != null)
						{
							int scoreReduction = ancientVaultHelper.GetExtraIncludedRocketScore(false, column);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// ForceField道具处理
					if (levelManager.IsThereItemInLevel(StaticItemType.ForceField)) // StaticItemType.ForceField = 15
					{
						var forceFieldHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.ForceFieldItem.ForceFieldItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ForceFieldItemHelper);
						if (forceFieldHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = forceFieldHelper.GetForceFieldScoreOffsetForPropellerCombo(GuidedExploderType.VerticalRocket, column, defaultPoint);
							columnScores[column] += extraScore;
						}
					}
					
					// Log道具处理
					if (levelManager.IsThereItemInLevel(ItemType.LogCollect)) // ItemType.LogCollect = 79
					{
						var logHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.LogItem.LogItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.LogItemHelper);
						if (logHelper != null)
						{
							int scoreReduction = logHelper.GetExtraIncludedRocketScore(false, column);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// PowerCube道具处理
					if (levelManager.IsThereItemInLevel(ItemType.PowerCube)) // ItemType.PowerCube = 85
					{
						var powerCubeHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PowerCubeItem.PowerCubeItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PowerCubeItemHelper);
						if (powerCubeHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = powerCubeHelper.GetPowerCubeScoreOffsetForPropellerCombo(GuidedExploderType.VerticalRocket, column, defaultPoint, isWaterLevel);
							columnScores[column] += extraScore;
						}
					}
					
					// PotionTube道具处理
					if (levelManager.IsThereItemInLevel(ItemType.PotionTube)) // ItemType.PotionTube = 86
					{
						var potionTubeHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PotionTubeItem.PotionTubeItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PotionTubeItemHelper);
						if (potionTubeHelper != null)
						{
							int scoreReduction = potionTubeHelper.GetVerticalLogFakeItemScoreToBeReduced(column, isWaterLevel);
							columnScores[column] -= scoreReduction;
						}
					}
					
					// ChocoMaker道具额外处理
					if (levelManager.IsThereItemInLevel(ItemType.ChocoMaker)) // ItemType.ChocoMaker = 93
					{
						var chocoMakerHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.ChocoMakerItem.ChocoMakerItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ChocoMakerItemHelper);
						if (chocoMakerHelper != null)
						{
							int scoreReduction = chocoMakerHelper.GetExtraIncludedRocketScore(false, column);
							columnScores[column] -= scoreReduction;
						}
					}
				}
			}
			
			// 选择目标列
			int targetColumn;
			if (winningColumnsOrRows != null && winningColumnsOrRows.Count > 0)
			{
				// 从胜利列表中随机选择
				if (randomManager != null)
				{
					targetColumn = randomManager.GetRandomItemFromList(winningColumnsOrRows);
				}
				else
				{
					return null;
				}
			}
			else
			{
				// 找到最高分数的列
				targetColumn = FindHighestScoreForRocket(columnScores);
				
				// 如果最高分数列的分数 <= 0，尝试找有正分数的列
				if (columnScores[targetColumn] <= 0)
				{
					int bestScore = int.MinValue;
					int bestColumn = targetColumn;
					
					for (int i = 0; i < columnScores.Length; i++)
					{
						if (hasAddedPositiveColumnScore != null && 
							hasAddedPositiveColumnScore[i] && 
							columnScores[i] > bestScore)
						{
							bestScore = columnScores[i];
							bestColumn = i;
						}
					}
					
					targetColumn = bestColumn;
				}
			}
			
			// 构建目标列的单元格列表
			var targetCells = new Royal.Scenes.Game.Mechanics.Matches.Cell14();
			
			if (gridManager != null)
			{
				int startY = gridManager.BoardStartY;
				int endY = gridManager.BoardEndY;
				
				for (int y = startY; y <= endY; y++)
				{
					var cellPoint = new CellPoint(targetColumn, y);
					var cell = gridManager[cellPoint];
					if (cell != null)
					{
						targetCells.Add(cell);
					}
				}
			}
			
			// 打乱单元格顺序
			if (randomManager != null)
			{
				var shuffledCells = new Royal.Scenes.Game.Mechanics.Matches.Cell14();
				targetCells.CopyTo(shuffledCells);
				randomManager.ShuffleCells(shuffledCells);
				targetCells = shuffledCells;
			}
			
			// 优先选择果冻传播目标
			if (canExploderSpreadJelly)
			{
				var jellyTarget = GetPrioritizedCellForJellySpread(canExploderSpreadJelly, targetCells);
				if (jellyTarget?.ExplodeTargetMediator != null)
				{
					return jellyTarget.ExplodeTargetMediator.AddIncomingExploder(exploder);
				}
			}
			
			// 优先选择链底部收集目标
			var chainBottomTarget = GetPrioritizedCellInColumnForChainBottomCollect(guidedExploderType, targetCells);
			if (chainBottomTarget?.ExplodeTargetMediator != null)
			{
				return chainBottomTarget.ExplodeTargetMediator.AddIncomingExploder(exploder);
			}
			
			// 寻找有效目标
			for (int i = 0; i < targetCells.Count; i++)
			{
				var cell = targetCells[i];
				if (cell == null) continue;
				
				// 检查是否有灯泡下方的可目标道具
				if (HasTargetableBelowItemUnderLightball(cell))
				{
					if (cell.ExplodeTargetMediator != null)
					{
						return cell.ExplodeTargetMediator.AddIncomingExploder(exploder);
					}
				}
				
				// 检查是否有足够的分数
				if (cell.ExplodeTargetMediator != null)
				{
					int cellScore = cell.ExplodeTargetMediator.GetScore(guidedExploderType, canExploderSpreadJelly, null);
					if (cellScore >= 1)
					{
						return cell.ExplodeTargetMediator.AddIncomingExploder(exploder);
					}
				}
			}
			
			// 寻找特殊道具目标
			Royal.Scenes.Game.Mechanics.Items.ItemModel bestSpecialItem = null;
			CellModel bestSpecialCell = null;
			
			for (int i = 0; i < targetCells.Count; i++)
			{
				var cell = targetCells[i];
				if (cell == null || cell.IsBlankCell()) continue;
				
				var currentItem = cell.CurrentItem;
				if (currentItem != null && currentItem.IsSpecialItem())
				{
					bestSpecialItem = currentItem;
					bestSpecialCell = cell;
					break;
				}
			}
			
			if (bestSpecialCell?.ExplodeTargetMediator != null)
			{
				return bestSpecialCell.ExplodeTargetMediator.AddIncomingExploder(exploder);
			}
			
			// 记录错误并返回null
			Royal.Infrastructure.Services.Logs.Log.Error(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, "No valid column target found", new object[0]);
			return null;
		}

		// Token: 0x0600E057 RID: 57431 RVA: 0x00002050 File Offset: 0x00000250
		[Token(Token = "0x600E057")]
		[Address(RVA = "0x1AC0CBC", Offset = "0x1AC0CBC", VA = "0x1AC0CBC")]
		private IExplodeTarget FindRowTarget(ExplodeData exploder, GuidedExploderType guidedExploderType, bool canExploderSpreadJelly, bool isForDebugDisplay = false)
		{
			// 清空胜利列/行列表
			if (winningColumnsOrRows != null)
			{
				winningColumnsOrRows.Clear();
			}
			
			// 检查是否有ChocoMaker道具并更新额外分数
			if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.ChocoMaker)) // ItemType.ChocoMaker = 93
			{
				var chocoMakerHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.ChocoMakerItem.ChocoMakerItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ChocoMakerItemHelper);
				if (chocoMakerHelper != null && rowScores != null)
				{
					chocoMakerHelper.UpdateExtraScoresForChocoMakers(guidedExploderType, rowScores.Length);
				}
			}
			
			if (rowScores == null)
				return null;
				
			// 遍历所有行计算分数
			for (int row = 0; row < rowScores.Length; row++)
			{
				// 清空目标依赖计数数组
				if (goalDependentCounts != null)
				{
					System.Array.Clear(goalDependentCounts, 0, goalDependentCounts.Length);
				}
				
				if (gridManager == null)
					return null;
					
				int actualRow = gridManager.BoardStartY + row;
				
				// 计算行分数
				bool hasAddedPositiveScoreItem;
				int score = GetRowScore(actualRow, canExploderSpreadJelly, out hasAddedPositiveScoreItem);
				
				// 存储分数
				rowScores[row] = score;
				
				// 存储是否有正分数项
				if (hasAddedPositiveRowScore != null)
				{
					hasAddedPositiveRowScore[row] = hasAddedPositiveScoreItem;
				}
				
				// 检查是否应该添加到胜利列表
				if (winCalculationData != null && winCalculationData.CheckRow(actualRow, exploder))
				{
					// 记录到日志
					var logValues = new object[] { row };
					Royal.Infrastructure.Services.Logs.Log.Debug(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, "Row added to winning list", logValues);
					
					// 添加到胜利列表
					if (winningColumnsOrRows != null)
					{
						winningColumnsOrRows.Add(row);
					}
				}
				
				// 如果不能传播果冻，添加螺旋桨水平火箭组合额外分数
				if (!exploder.canSpreadJelly)
				{
					int extraScore = Royal.Scenes.Game.Mechanics.Items.StaticItems.JellyItem.JellyItemModel.GetPropellerHorizontalRocketComboExtraScore(actualRow);
					rowScores[row] += extraScore;
				}
				
				// 各种道具的额外分数处理
				if (levelManager != null)
				{
					// IceCrusher道具处理
					if (levelManager.IsThereItemInLevel(ItemType.IceCrusher)) // ItemType.IceCrusher = 19
					{
						var iceCrusherHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.IceCrusherItem.IceCrusherItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.IceCrusherItemHelper);
						if (iceCrusherHelper != null)
						{
							int scoreReduction = iceCrusherHelper.GetHorizontalLogFakeItemScoreToBeReduced(row, isWaterLevel);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// MetalCrusher道具处理
					if (levelManager.IsThereItemInLevel(ItemType.MetalCrusher)) // ItemType.MetalCrusher = 32
					{
						var metalCrusherHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.MetalCrusherItem.MetalCrusherItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.MetalCrusherItemHelper);
						if (metalCrusherHelper != null)
						{
							int scoreReduction = metalCrusherHelper.GetHorizontalLogFakeItemScoreToBeReduced(row, isWaterLevel);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// Soil道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Soil)) // ItemType.Soil = 28
					{
						var soilHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.SoilItem.SoilItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.SoilItemHelper);
						if (soilHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = soilHelper.GetSoilScoreOffsetForPropellerCombo(GuidedExploderType.HorizontalRocket, actualRow, defaultPoint, isWaterLevel);
							rowScores[row] += extraScore;
						}
					}
					
					// Magnet道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Magnet)) // ItemType.Magnet = 56
					{
						var magnetHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.MagnetItem.MagnetItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.MagnetItemHelper);
						if (magnetHelper != null)
						{
							var defaultPoint = new CellPoint();
							int scoreReduction = magnetHelper.GetMultipleHeadScoreToBeReduced(actualRow, guidedExploderType, defaultPoint, isWaterLevel);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// CandyCane道具处理
					if (levelManager.IsThereItemInLevel(ItemType.CandyCane)) // ItemType.CandyCane = 57
					{
						var candyCaneHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CandyCaneItem.CandyCaneItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.CandyCaneItemHelper);
						if (candyCaneHelper != null)
						{
							int scoreReduction = candyCaneHelper.GetScoreDifferenceForRocketCombo(actualRow, true);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// Coil道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Coil)) // ItemType.Coil = 61
					{
						var coilHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CoilItem.CoilItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.CoilItemHelper);
						if (coilHelper != null)
						{
							int scoreReduction = coilHelper.GetExtraIncludedRocketScore(true, row);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// CookieJar道具处理
					if (levelManager.IsThereItemInLevel(ItemType.CookieJar)) // ItemType.CookieJar = 33
					{
						var cookieJarHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CookieJarItem.View.CookieJarCollectHelper>(Royal.Scenes.Game.Levels.LevelContextId.CookieJarCollectHelper);
						if (cookieJarHelper != null)
						{
							int scoreReduction = cookieJarHelper.GetExtraIncludedRocketScore(true, row);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// Ufo道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Ufo)) // ItemType.Ufo = 45
					{
						var ufoHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.UfoItem.View.UfoCollectHelper>(Royal.Scenes.Game.Levels.LevelContextId.UfoCollectHelper);
						if (ufoHelper != null)
						{
							int scoreReduction = ufoHelper.GetExtraIncludedRocketScore(true, row);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// Ivy道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Ivy)) // ItemType.Ivy = 66
					{
						var ivyHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.IvyItem.IvyItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.IvyItemHelper);
						if (ivyHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = ivyHelper.GetIvyScoreOffsetForPropellerCombo(GuidedExploderType.HorizontalRocket, actualRow, defaultPoint, isWaterLevel);
							rowScores[row] += extraScore;
						}
					}
					
					// Blinds道具处理
					if (levelManager.IsThereItemInLevel(StaticItemType.Blinds)) // StaticItemType.Blinds = 11
					{
						var blindsHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.BlindsItem.BlindsItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.BlindsItemHelper);
						if (blindsHelper != null)
						{
							int scoreReduction = blindsHelper.GetHorizontalScoreToBeReduced(row, isWaterLevel);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// Porcelain道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Porcelain)) // ItemType.Porcelain = 70
					{
						var porcelainHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PorcelainItem.PorcelainItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PorcelainItemHelper);
						if (porcelainHelper != null)
						{
							int scoreReduction = porcelainHelper.GetHorizontalPorcelainFakeItemScoreToBeReduced(row, isWaterLevel);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// AncientVault道具处理
					if (levelManager.IsThereItemInLevel(ItemType.AncientVault)) // ItemType.AncientVault = 75
					{
						var ancientVaultHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.AncientVaultItem.AncientVaultItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.AncientVaultItemHelper);
						if (ancientVaultHelper != null)
						{
							int scoreReduction = ancientVaultHelper.GetExtraIncludedRocketScore(true, row);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// ForceField道具处理
					if (levelManager.IsThereItemInLevel(StaticItemType.ForceField)) // StaticItemType.ForceField = 15
					{
						var forceFieldHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.ForceFieldItem.ForceFieldItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ForceFieldItemHelper);
						if (forceFieldHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = forceFieldHelper.GetForceFieldScoreOffsetForPropellerCombo(GuidedExploderType.HorizontalRocket, actualRow, defaultPoint);
							rowScores[row] += extraScore;
						}
					}
					
					// Log道具处理
					if (levelManager.IsThereItemInLevel(ItemType.LogCollect)) // ItemType.LogCollect = 79
					{
						var logHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.LogItem.LogItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.LogItemHelper);
						if (logHelper != null)
						{
							int scoreReduction = logHelper.GetExtraIncludedRocketScore(true, row);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// PowerCube道具处理
					if (levelManager.IsThereItemInLevel(ItemType.PowerCube)) // ItemType.PowerCube = 85
					{
						var powerCubeHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PowerCubeItem.PowerCubeItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PowerCubeItemHelper);
						if (powerCubeHelper != null)
						{
							var defaultPoint = new CellPoint();
							int extraScore = powerCubeHelper.GetPowerCubeScoreOffsetForPropellerCombo(GuidedExploderType.HorizontalRocket, actualRow, defaultPoint, isWaterLevel);
							rowScores[row] += extraScore;
						}
					}
					
					// PotionTube道具处理
					if (levelManager.IsThereItemInLevel(ItemType.PotionTube)) // ItemType.PotionTube = 86
					{
						var potionTubeHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PotionTubeItem.PotionTubeItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PotionTubeItemHelper);
						if (potionTubeHelper != null)
						{
							int scoreReduction = potionTubeHelper.GetHorizontalLogFakeItemScoreToBeReduced(row, isWaterLevel);
							rowScores[row] -= scoreReduction;
						}
					}
					
					// ChocoMaker道具额外处理
					if (levelManager.IsThereItemInLevel(ItemType.ChocoMaker)) // ItemType.ChocoMaker = 93
					{
						var chocoMakerHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.ChocoMakerItem.ChocoMakerItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ChocoMakerItemHelper);
						if (chocoMakerHelper != null)
						{
							int scoreReduction = chocoMakerHelper.GetExtraIncludedRocketScore(true, row);
							rowScores[row] -= scoreReduction;
						}
					}
				}
			}
			
			// 选择目标行
			int targetRow;
			if (winningColumnsOrRows != null && winningColumnsOrRows.Count > 0)
			{
				// 从胜利列表中随机选择
				if (randomManager != null)
				{
					targetRow = randomManager.GetRandomItemFromList(winningColumnsOrRows);
				}
				else
				{
					return null;
				}
			}
			else
			{
				// 找到最高分数的行
				targetRow = FindHighestScoreForRocket(rowScores);
				
				// 如果最高分数行的分数 <= 0，尝试找有正分数的行
				if (rowScores[targetRow] <= 0)
				{
					int bestScore = int.MinValue;
					int bestRow = targetRow;
					
					for (int i = 0; i < rowScores.Length; i++)
					{
						if (hasAddedPositiveRowScore != null && 
							hasAddedPositiveRowScore[i] && 
							rowScores[i] > bestScore)
						{
							bestScore = rowScores[i];
							bestRow = i;
						}
					}
					
					targetRow = bestRow;
				}
			}
			
			// 构建目标行的单元格列表
			var targetCells = new Royal.Scenes.Game.Mechanics.Matches.Cell14();
			
			if (gridManager != null)
			{
				int startX = 0;
				int endX = gridManager.Width - 1;
				int actualRow = gridManager.BoardStartY + targetRow;
				
				for (int x = startX; x <= endX; x++)
				{
					var cellPoint = new CellPoint(x, actualRow);
					var cell = gridManager[cellPoint];
					if (cell != null)
					{
						targetCells.Add(cell);
					}
				}
			}
			
			// 打乱单元格顺序
			if (randomManager != null)
			{
				var shuffledCells = new Royal.Scenes.Game.Mechanics.Matches.Cell14();
				targetCells.CopyTo(shuffledCells);
				randomManager.ShuffleCells(shuffledCells);
				targetCells = shuffledCells;
			}
			
			// 优先选择果冻传播目标
			if (canExploderSpreadJelly)
			{
				var jellyTarget = GetPrioritizedCellForJellySpread(canExploderSpreadJelly, targetCells);
				if (jellyTarget?.ExplodeTargetMediator != null)
				{
					return jellyTarget.ExplodeTargetMediator.AddIncomingExploder(exploder);
				}
			}
			
			// 寻找有效目标
			for (int i = 0; i < targetCells.Count; i++)
			{
				var cell = targetCells[i];
				if (cell == null) continue;
				
				// 检查是否有灯泡下方的可目标道具
				if (HasTargetableBelowItemUnderLightball(cell))
				{
					if (cell.ExplodeTargetMediator != null)
					{
						return cell.ExplodeTargetMediator.AddIncomingExploder(exploder);
					}
				}
				
				// 检查是否有足够的分数
				if (cell.ExplodeTargetMediator != null)
				{
					int cellScore = cell.ExplodeTargetMediator.GetScore(guidedExploderType, canExploderSpreadJelly, null);
					if (cellScore >= 1)
					{
						return cell.ExplodeTargetMediator.AddIncomingExploder(exploder);
					}
				}
			}
			
			// 寻找特殊道具目标
			Royal.Scenes.Game.Mechanics.Items.ItemModel bestSpecialItem = null;
			CellModel bestSpecialCell = null;
			
			for (int i = 0; i < targetCells.Count; i++)
			{
				var cell = targetCells[i];
				if (cell == null || cell.IsBlankCell()) continue;
				
				var currentItem = cell.CurrentItem;
				if (currentItem != null && currentItem.IsSpecialItem())
				{
					bestSpecialItem = currentItem;
					bestSpecialCell = cell;
					break;
				}
			}
			
			if (bestSpecialCell?.ExplodeTargetMediator != null)
			{
				return bestSpecialCell.ExplodeTargetMediator.AddIncomingExploder(exploder);
			}
			
			// 记录错误并返回null
			Royal.Infrastructure.Services.Logs.Log.Error(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, "No valid row target found", new object[0]);
			return null;
		}

		// Token: 0x0600E058 RID: 57432 RVA: 0x00048468 File Offset: 0x00046668
		[Token(Token = "0x600E058")]
		[Address(RVA = "0x1AC2E60", Offset = "0x1AC2E60", VA = "0x1AC2E60")]
		public CellPoint FindHighestScoreForAreaExploder(ExplodeData exploder, bool canExploderSpreadJelly, bool isForDebugDisplay = false, [Optional] IComparer<CellPoint> customComparer)
		{
			// 初始化hasAddedPositiveScoreItem为false
			bool hasAddedPositiveScoreItem = false;
			
			// 将触发器转换为GuidedExploderType
			var guidedExploderType = TriggerExtensions.AsGuidedExploderType(exploder.trigger);
			
			// 检查是否为多重爆炸器
			bool isMultipleExploder = GameplayExtensions.IsMultipleExploder(guidedExploderType);
			
			// 清空区域分数点列表
			if (areaScorePointList != null)
			{
				areaScorePointList.Clear();
			}
			else
			{
				return CellPoint.Default;
			}
			
			// 重置迭代器
			iterator.Reset();
			
			// 检查是否有ChocoMaker道具并更新额外TNT分数
			if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.ChocoMaker)) // ItemType.ChocoMaker = 93
			{
				var chocoMakerHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.ChocoMakerItem.ChocoMakerItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ChocoMakerItemHelper);
				if (chocoMakerHelper != null)
				{
					chocoMakerHelper.UpdateExtraTntScoresForChocoMakers();
				}
			}
			
			// 初始化最高分数
			int highestScore = int.MinValue;
			bool hasWinningPoint = false;
			
			// 遍历所有单元格
			while (iterator.MoveNext())
			{
				var cell = iterator.Cell;
				if (cell == null)
					continue;
				
				// 只处理普通单元格
				if (!cell.IsNormalCell())
					continue;
				
				// 检查是否有灯泡下方的可目标道具
				bool hasTargetableBelowItem = HasTargetableBelowItemUnderLightball(cell);
				
				// 检查果冻传播能力
				bool canSpreadJelly = false;
				if (isMultipleExploder)
				{
					if (cell.WillBeAbleToSpreadJelly())
					{
						canSpreadJelly = true;
					}
					else
					{
						canSpreadJelly = cell.CanSpreadJelly(true);
					}
				}
				
				// 如果有可目标的下方道具，直接处理
				if (hasTargetableBelowItem)
				{
					goto ProcessCell;
				}
				
				// 检查是否有爆炸目标中介器且分数大于0
				if (cell.ExplodeTargetMediator != null)
				{
					int score = cell.ExplodeTargetMediator.GetScore(guidedExploderType, canExploderSpreadJelly, cell);
					if (score > 0)
					{
						goto ProcessCell;
					}
				}
				
				// 检查是否能传播果冻
				if (canSpreadJelly || cell.CanSpreadJelly(false))
				{
					goto ProcessCell;
				}
				
				// 检查ChocoMaker特殊情况
				if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.ChocoMaker))
				{
					if (cell.StaticMediator != null && !cell.StaticMediator.HasAboveItem())
					{
						var currentItem = cell.CurrentItem;
						if (currentItem is Royal.Scenes.Game.Mechanics.Items.ChocoMakerItem.ChocoMakerItemModel)
						{
							goto ProcessCell;
						}
					}
				}
				
				continue;
				
				ProcessCell:
				// 计算单元格的区域爆炸器分数
				int cellScore = CalculateScoreForAreaExploder(cell.Point, canExploderSpreadJelly, exploder.trigger, out hasAddedPositiveScoreItem);
				
				// 如果分数为0且没有正分数项，检查是否有果冻道具
				if (cellScore == 0 && !hasAddedPositiveScoreItem)
				{
					if (levelManager != null && levelManager.IsThereItemInLevel(StaticItemType.Jelly)) // StaticItemType.Jelly = 3
					{
						continue;
					}
				}
				
				// 应用各种道具的额外分数调整
				if (levelManager != null)
				{
					// Coil道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Coil)) // ItemType.Coil = 61
					{
						var coilHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CoilItem.CoilItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.CoilItemHelper);
						if (coilHelper != null)
						{
							var adjustedPoint = cell.GetBoardAdjustedPoint();
							cellScore -= coilHelper.GetExtraIncludedTntScore(adjustedPoint);
						}
					}
					
					// CookieJar道具处理
					if (levelManager.IsThereItemInLevel(ItemType.CookieJar)) // ItemType.CookieJar = 33
					{
						var cookieJarHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.CookieJarItem.View.CookieJarCollectHelper>(Royal.Scenes.Game.Levels.LevelContextId.CookieJarCollectHelper);
						if (cookieJarHelper != null)
						{
							var adjustedPoint = cell.GetBoardAdjustedPoint();
							cellScore -= cookieJarHelper.GetExtraIncludedTntScore(adjustedPoint);
						}
					}
					
					// Ufo道具处理
					if (levelManager.IsThereItemInLevel(ItemType.Ufo)) // ItemType.Ufo = 45
					{
						var ufoHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.UfoItem.View.UfoCollectHelper>(Royal.Scenes.Game.Levels.LevelContextId.UfoCollectHelper);
						if (ufoHelper != null)
						{
							var adjustedPoint = cell.GetBoardAdjustedPoint();
							cellScore -= ufoHelper.GetExtraIncludedTntScore(adjustedPoint);
						}
					}
					
					// AncientVault道具处理
					if (levelManager.IsThereItemInLevel(ItemType.AncientVault)) // ItemType.AncientVault = 75
					{
						var ancientVaultHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.AncientVaultItem.AncientVaultItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.AncientVaultItemHelper);
						if (ancientVaultHelper != null)
						{
							var adjustedPoint = cell.GetBoardAdjustedPoint();
							cellScore -= ancientVaultHelper.GetExtraIncludedTntScore(adjustedPoint);
						}
					}
					
					// Log道具处理
					if (levelManager.IsThereItemInLevel(ItemType.LogCollect)) // ItemType.LogCollect = 79
					{
						var logHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.LogItem.LogItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.LogItemHelper);
						if (logHelper != null)
						{
							var adjustedPoint = cell.GetBoardAdjustedPoint();
							cellScore -= logHelper.GetExtraIncludedTntScore(adjustedPoint);
						}
					}
					
					// ChocoMaker道具处理
					if (levelManager.IsThereItemInLevel(ItemType.ChocoMaker)) // ItemType.ChocoMaker = 93
					{
						var chocoMakerHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.ChocoMakerItem.ChocoMakerItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ChocoMakerItemHelper);
						if (chocoMakerHelper != null)
						{
							var adjustedPoint = cell.GetBoardAdjustedPoint();
							cellScore -= chocoMakerHelper.GetExtraIncludedTntScore(adjustedPoint);
						}
					}
				}
				
				// 检查是否为胜利条件
				bool isWinningCondition = false;
				if (winCalculationData != null)
				{
					isWinningCondition = winCalculationData.CheckPoint(cell.Point, exploder);
				}
				
				if (isWinningCondition)
				{
					// 记录胜利点到日志
					var logValues = new object[] { cell.Point };
					Royal.Infrastructure.Services.Logs.Log.Debug(this, Royal.Infrastructure.Services.Logs.LogTag.ExplodeTargetFinder, "Point added to winning list", logValues);
					
					// 如果是第一个胜利点，清空列表
					if (!hasWinningPoint)
					{
						areaScorePointList.Clear();
						hasWinningPoint = true;
					}
					
					// 添加到区域分数点列表
					areaScorePointList.Add(cell.Point);
				}
				else if (!hasWinningPoint)
				{
					// 如果没有胜利点，按分数处理
					bool shouldAdd = hasTargetableBelowItem || cellScore >= 0;
					
					if (cellScore == highestScore && shouldAdd)
					{
						// 分数相等，添加到候选列表
						areaScorePointList.Add(cell.Point);
					}
					else if (cellScore > highestScore && shouldAdd)
					{
						// 找到更高分数，清空列表并添加新点
						highestScore = cellScore;
						areaScorePointList.Clear();
						areaScorePointList.Add(cell.Point);
					}
				}
			}
			
			// 如果有候选点，随机选择一个
			if (areaScorePointList != null && areaScorePointList.Count > 0)
			{
				return GetRandomItemFromAreaScores(customComparer);
			}
			
			// 没有找到合适的点，返回默认值
			return CellPoint.Default;
		}

		// Token: 0x0600E059 RID: 57433 RVA: 0x00048480 File Offset: 0x00046680
		[Token(Token = "0x600E059")]
		[Address(RVA = "0x1AC732C", Offset = "0x1AC732C", VA = "0x1AC732C")]
		private CellPoint GetRandomItemFromAreaScores(IComparer<CellPoint> customComparer)
		{
			// 如果没有自定义比较器，使用随机管理器从列表中随机选择
			if (customComparer == null)
			{
				// 检查随机管理器是否存在
				if (randomManager != null)
				{
					// 从区域分数点列表中随机选择一个点
					return randomManager.GetRandomItemFromList(areaScorePointList);
				}
				
				// 如果随机管理器不存在，返回默认值
				return CellPoint.Default;
			}
			
			// 如果有自定义比较器，先打乱列表
			Royal.Infrastructure.Utils.Extensions.ListExtensions.Shuffle(areaScorePointList, randomManager);
			
			// 检查列表是否存在
			if (areaScorePointList == null)
				return CellPoint.Default;
			
			// 使用自定义比较器对列表进行排序
			areaScorePointList.Sort(customComparer);
			
			// 检查列表是否存在（排序后再次检查）
			if (areaScorePointList == null)
				return CellPoint.Default;
			
			// 返回排序后的第一个元素
			return areaScorePointList[0];
		}

		// Token: 0x0600E05A RID: 57434 RVA: 0x00048498 File Offset: 0x00046698
		[Token(Token = "0x600E05A")]
		[Address(RVA = "0x1AC5E50", Offset = "0x1AC5E50", VA = "0x1AC5E50")]
		public int CalculateScoreForAreaExploder(CellPoint point, bool canExploderSpreadJelly, Trigger trigger, out bool hasAddedPositiveScoreItem)
		{
			// 清空爆炸后清除的单元格标记数组
			if (clearedCellsAfterExploderHit != null)
			{
				System.Array.Clear(clearedCellsAfterExploderHit, 0, clearedCellsAfterExploderHit.Length);
			}
			
			// 初始化hasAddedPositiveScoreItem为false
			hasAddedPositiveScoreItem = false;
			
			// 清空目标依赖计数数组
			if (goalDependentCounts != null)
			{
				System.Array.Clear(goalDependentCounts, 0, goalDependentCounts.Length);
			}
			
			// 转换触发器类型为GuidedExploderType
			var guidedExploderType = Royal.Scenes.Game.Mechanics.Explode.TriggerExtensions.AsGuidedExploderType(trigger);
			
			// 检查是否为多重爆炸器
			bool isMultipleExploder = Royal.Scenes.Game.Mechanics.Items.Config.GameplayExtensions.IsMultipleExploder(guidedExploderType);
			
			// 获取爆炸半径
			int explodeRadius = Royal.Scenes.Game.Mechanics.Items.Config.GameplayExtensions.ExplodeRadius(guidedExploderType);
			
			int totalScore = 0;
			int treasureMapHitCount = 0; // 宝藏地图击中次数
			int logHitCount = 0; // 日志击中次数
			
			// 创建各种道具的集合用于收集
			System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.IceCrusherItem.IceCrusherItemModel> iceCrusherItems = null;
			System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.PotionTubeItem.PotionTubeItemModel> potionTubeItems = null;
			System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.MetalCrusherItem.MetalCrusherItemModel> metalCrusherItems = null;
			System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.CandyCaneItem.CandyCaneItemModel> candyCaneItems = null;
			System.Collections.Generic.HashSet<int> candyCaneIndexes = null;
			Royal.Scenes.Game.Mechanics.Items.CandyCaneItem.CandyCaneItemModel singleCandyCane = null;
			System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.PorcelainItem.PorcelainItemModel> porcelainItems = null;
			
			// 遍历爆炸范围内的所有单元格
			for (int deltaY = -explodeRadius; deltaY <= explodeRadius; deltaY++)
			{
				for (int deltaX = -explodeRadius; deltaX <= explodeRadius; deltaX++)
				{
					// 计算当前检查的单元格坐标
					var checkPoint = new CellPoint(point.x + deltaX, point.y + deltaY);
					
					// 获取单元格
					if (gridManager == null)
						continue;
						
					var cell = gridManager[checkPoint];
					if (cell == null)
						continue;
					
					// 检查是否有上方道具，如果有且不是普通道具则跳过
					if (cell.StaticMediator != null && cell.StaticMediator.HasAboveItem())
					{
						var topMostAboveItem = cell.StaticMediator.GetTopMostAboveItem();
						if (topMostAboveItem != null && !topMostAboveItem.IsNormalItem())
						{
							continue;
						}
					}
					
					// 检查是否有当前道具
					if (cell.Mediator == null || !cell.Mediator.HasCurrentItem())
					{
						// 没有当前道具时的处理
						bool hasPositiveScore = false;
						goto ProcessCellScore;
					}
					
					var currentItem = cell.CurrentItem;
					if (currentItem == null)
						continue;
					
					// 如果是底部收集关卡，更新清除单元格数据
					if (levelManager != null && levelManager.IsThereBottomCollectInLevel())
					{
						currentItem.UpdateClearedCellPointsAfterExploderHit(guidedExploderType, clearedCellsAfterExploderHit, point);
					}
					
					// 检查道具类型并收集特定道具
					var itemType = currentItem.ItemType;
					bool hasPositiveScore = itemType == ItemType.Lightball; // Lightball = 5
					
					// 根据不同道具类型进行处理
					switch (itemType)
					{
						case ItemType.IceCrusher: // 19
							// 收集IceCrusher道具
							if (iceCrusherItems == null)
								iceCrusherItems = new System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.IceCrusherItem.IceCrusherItemModel>();
							
							if (currentItem is Royal.Scenes.Game.Mechanics.Items.IceCrusherItem.IceCrusherFakeItemModel fakeIceCrusher)
							{
								iceCrusherItems.Add(fakeIceCrusher.ParentItemModel);
							}
							else if (currentItem is Royal.Scenes.Game.Mechanics.Items.IceCrusherItem.IceCrusherItemModel iceCrusher)
							{
								iceCrusherItems.Add(iceCrusher);
							}
							break;
							
						case ItemType.PotionTube: // 86
							// 收集PotionTube道具
							if (potionTubeItems == null)
								potionTubeItems = new System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.PotionTubeItem.PotionTubeItemModel>();
							
							if (currentItem is Royal.Scenes.Game.Mechanics.Items.PotionTubeItem.PotionTubeFakeItemModel fakePotionTube)
							{
								potionTubeItems.Add(fakePotionTube.ParentItemModel);
							}
							else if (currentItem is Royal.Scenes.Game.Mechanics.Items.PotionTubeItem.PotionTubeItemModel potionTube)
							{
								potionTubeItems.Add(potionTube);
							}
							break;
							
						case ItemType.MetalCrusher: // 32
							// 收集MetalCrusher道具
							if (metalCrusherItems == null)
								metalCrusherItems = new System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.MetalCrusherItem.MetalCrusherItemModel>();
							
							if (currentItem is Royal.Scenes.Game.Mechanics.Items.MetalCrusherItem.MetalCrusherFakeItemModel fakeMetalCrusher)
							{
								metalCrusherItems.Add(fakeMetalCrusher.ParentItemModel);
							}
							else if (currentItem is Royal.Scenes.Game.Mechanics.Items.MetalCrusherItem.MetalCrusherItemModel metalCrusher)
							{
								metalCrusherItems.Add(metalCrusher);
							}
							break;
							
						case ItemType.CandyCane: // 57
							// 处理CandyCane道具
							if (currentItem is Royal.Scenes.Game.Mechanics.Items.CandyCaneItem.CandyCaneItemModel candyCane)
							{
								if (candyCane.isLeader)
								{
									singleCandyCane = candyCane;
								}
								else
								{
									// 收集非leader的CandyCane
									if (candyCaneItems == null)
									{
										candyCaneIndexes = new System.Collections.Generic.HashSet<int>();
										candyCaneItems = new System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.CandyCaneItem.CandyCaneItemModel>();
									}
									
									if (candyCaneIndexes.Add(candyCane.Index))
									{
										candyCaneItems.Add(candyCane);
									}
								}
							}
							break;
							
						case ItemType.Porcelain: // 70
							// 收集Porcelain道具
							if (porcelainItems == null)
								porcelainItems = new System.Collections.Generic.HashSet<Royal.Scenes.Game.Mechanics.Items.PorcelainItem.PorcelainItemModel>();
							
							if (currentItem is Royal.Scenes.Game.Mechanics.Items.PorcelainItem.PorcelainFakeItemModel fakePorcelain)
							{
								porcelainItems.Add(fakePorcelain.ParentItemModel);
							}
							else if (currentItem is Royal.Scenes.Game.Mechanics.Items.PorcelainItem.PorcelainItemModel porcelain)
							{
								porcelainItems.Add(porcelain);
							}
							break;
							
						case ItemType.MapScrollCollect: // 48
							// 检查宝藏地图剩余爆炸次数
							if (currentItem.RemainingExplodeCount() <= 1)
							{
								treasureMapHitCount++;
							}
							break;
							
						case ItemType.LogCollect: // 80
							// 检查日志剩余爆炸次数
							if (currentItem.RemainingExplodeCount() < 2)
							{
								logHitCount++;
							}
							break;
					}
					
					ProcessCellScore:
					// 获取中心点的单元格用于传播果冻检查
					var centerCell = gridManager[point];
					bool canSpreadJelly = false;
					if (centerCell != null)
					{
						canSpreadJelly = centerCell.CanSpreadJelly(false);
					}
					
					// 获取单元格的爆炸分数
					if (cell.ExplodeTargetMediator != null)
					{
						int cellScore = cell.ExplodeTargetMediator.GetScore(guidedExploderType, canExploderSpreadJelly || canSpreadJelly, centerCell);
						
						// 检查是否添加了正分数项
						if (cellScore > 0)
						{
							hasAddedPositiveScoreItem = true;
							if (hasPositiveScore) // 如果是Lightball
							{
								totalScore -= 1599395; // 0xFFE7985D的补码
							}
						}
						else
						{
							hasAddedPositiveScoreItem = hasAddedPositiveScoreItem || HasTargetableBelowItemUnderLightball(cell);
						}
						
						// 应用多重爆炸器的果冻传播奖励
						if (isMultipleExploder && 
							cell.HasJellySpreadBlockingItem(false) && 
							centerCell != null && centerCell.CanSpreadJelly(true) &&
							(cell.CanReceiveJelly(true) || cell.WillBeAbleToReceiveJelly()))
						{
							if (Royal.Scenes.Game.Mechanics.Items.StaticItems.JellyItem.JellyItemModel.IsThereAtLeastOneNeighborWithJelly(cell))
							{
								cellScore += 338106;
							}
							else
							{
								cellScore += 342675;
							}
						}
						
						// 如果分数不为0，检查是否应该添加到总分数
						if (cellScore != 0)
						{
							if (ShouldAddToTotalScore(cell))
							{
								totalScore += cellScore;
							}
						}
					}
				}
			}
			
			// 如果是底部收集关卡，添加额外分数
			if (levelManager != null && levelManager.IsThereBottomCollectInLevel())
			{
				if (gridManager != null)
				{
					int additionalScore = gridManager.GetBottomCollectAreaScoreForClearedCells(guidedExploderType, clearedCellsAfterExploderHit, point);
					totalScore += additionalScore;
				}
			}
			
			// 处理收集到的各种道具的额外分数
			// IceCrusher道具处理
			if (iceCrusherItems != null && iceCrusherItems.Count > 0)
			{
				foreach (var iceCrusher in iceCrusherItems)
				{
					if (isMultipleExploder && !iceCrusher.IsNormalItem())
						continue;
					
					float multiplier = isWaterLevel ? iceCrusher.GetMaxBoostMultiplier(guidedExploderType) : 1.0f;
					int itemScore = (int)(multiplier * iceCrusher.GetScore(guidedExploderType));
					totalScore += itemScore;
				}
			}
			
			// PotionTube道具处理
			if (potionTubeItems != null && potionTubeItems.Count > 0)
			{
				foreach (var potionTube in potionTubeItems)
				{
					if (isMultipleExploder && !potionTube.IsNormalItem())
						continue;
					
					float multiplier = isWaterLevel ? potionTube.GetMaxBoostMultiplier(guidedExploderType) : 1.0f;
					int itemScore = (int)(multiplier * potionTube.GetScore(guidedExploderType));
					totalScore += itemScore;
				}
			}
			
			// MetalCrusher道具处理
			if (metalCrusherItems != null && metalCrusherItems.Count > 0)
			{
				foreach (var metalCrusher in metalCrusherItems)
				{
					if (isMultipleExploder && !metalCrusher.IsNormalItem())
						continue;
					
					float multiplier = isWaterLevel ? metalCrusher.GetMaxBoostMultiplier(guidedExploderType) : 1.0f;
					int itemScore = (int)(multiplier * metalCrusher.GetScore(guidedExploderType));
					totalScore += itemScore;
				}
			}
			
			// CandyCane道具处理
			if (levelManager != null && levelManager.IsThereItemInLevel(ItemType.CandyCane))
			{
				if (candyCaneItems != null && candyCaneItems.Count > 0)
				{
					foreach (var candyCane in candyCaneItems)
					{
						candyCane.SetTargetCellPoint(point);
						totalScore += candyCane.GetScore(guidedExploderType);
					}
				}
				else if (singleCandyCane != null)
				{
					singleCandyCane.SetTargetCellPoint(point);
					totalScore += singleCandyCane.GetScore(guidedExploderType);
				}
			}
			
			// Porcelain道具处理
			if (porcelainItems != null && porcelainItems.Count > 0)
			{
				foreach (var porcelain in porcelainItems)
				{
					if (isMultipleExploder && !porcelain.IsNormalItem())
						continue;
					
					float multiplier = isWaterLevel ? porcelain.GetMaxBoostMultiplier(guidedExploderType) : 1.0f;
					int itemScore = (int)(multiplier * porcelain.GetScore(guidedExploderType));
					totalScore += itemScore;
				}
			}
			
			// 应用各种道具的额外分数修正
			if (levelManager != null)
			{
				// Soil道具处理
				if (levelManager.IsThereItemInLevel(ItemType.Soil))
				{
					var soilHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.SoilItem.SoilItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.SoilItemHelper);
					if (soilHelper != null)
					{
						int extraScore = soilHelper.GetSoilScoreOffsetForPropellerCombo(guidedExploderType, -1, point, isWaterLevel);
						totalScore += extraScore;
					}
				}
				
				// Magnet道具处理
				if (levelManager.IsThereItemInLevel(ItemType.Magnet))
				{
					var magnetHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.MagnetItem.MagnetItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.MagnetItemHelper);
					if (magnetHelper != null)
					{
						int scoreReduction = magnetHelper.GetMultipleHeadScoreToBeReduced(-1, guidedExploderType, point, isWaterLevel);
						totalScore -= scoreReduction;
					}
				}
				
				// Ivy道具处理
				if (levelManager.IsThereItemInLevel(ItemType.Ivy))
				{
					var ivyHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.IvyItem.IvyItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.IvyItemHelper);
					if (ivyHelper != null)
					{
						int extraScore = ivyHelper.GetIvyScoreOffsetForPropellerCombo(guidedExploderType, -1, point, isWaterLevel);
						totalScore += extraScore;
					}
				}
				
				// Blinds道具处理
				if (levelManager.IsThereItemInLevel(StaticItemType.Blinds))
				{
					var blindsHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.BlindsItem.BlindsItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.BlindsItemHelper);
					if (blindsHelper != null)
					{
						int scoreReduction = blindsHelper.GetScoreOffsetForAreaExploder(guidedExploderType, point, isWaterLevel);
						totalScore -= scoreReduction;
					}
				}
				
				// ForceField道具处理
				if (levelManager.IsThereItemInLevel(StaticItemType.ForceField))
				{
					var forceFieldHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.ForceFieldItem.ForceFieldItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.ForceFieldItemHelper);
					if (forceFieldHelper != null)
					{
						int extraScore = forceFieldHelper.GetForceFieldScoreOffsetForPropellerCombo(guidedExploderType, -1, point);
						totalScore += extraScore;
					}
				}
				
				// PowerCube道具处理
				if (levelManager.IsThereItemInLevel(ItemType.PowerCube))
				{
					var powerCubeHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.PowerCubeItem.PowerCubeItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.PowerCubeItemHelper);
					if (powerCubeHelper != null)
					{
						int extraScore = powerCubeHelper.GetPowerCubeScoreOffsetForPropellerCombo(guidedExploderType, -1, point, isWaterLevel);
						totalScore += extraScore;
					}
				}
			}
			
			// 添加宝藏地图额外分数
			if (treasureMapHitCount >= 1)
			{
				var treasureMapHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.StaticItems.TreasureMapItem.TreasureMapHelper>(Royal.Scenes.Game.Levels.LevelContextId.TreasureMapHelper);
				if (treasureMapHelper != null)
				{
					totalScore += treasureMapHelper.GetTreasureRevealExtraScore(treasureMapHitCount);
				}
			}
			
			// 添加日志额外分数
			if (logHitCount >= 1)
			{
				var logItemHelper = Royal.Scenes.Game.Levels.LevelContext.GetLate<Royal.Scenes.Game.Mechanics.Items.LogItem.LogItemHelper>(Royal.Scenes.Game.Levels.LevelContextId.LogItemHelper);
				if (logItemHelper != null)
				{
					totalScore += logItemHelper.GetLogActivateExtraScore(logHitCount);
				}
			}
			
			return totalScore;
		}

		// Token: 0x0600E05B RID: 57435 RVA: 0x000484B0 File Offset: 0x000466B0
		[Token(Token = "0x600E05B")]
		[Address(RVA = "0x1AC3FB0", Offset = "0x1AC3FB0", VA = "0x1AC3FB0")]
		private bool HasTargetableBelowItemUnderLightball(CellModel cell)
		{
			// 检查单元格是否为空
			if (cell == null)
				return false;
			
			// 检查静态中介器是否存在
			if (cell.StaticMediator == null)
				return false;
			
			// 如果有上方道具，返回false
			if (cell.StaticMediator.HasAboveItem())
				return false;
			
			// 检查中介器是否存在
			if (cell.Mediator == null)
				return false;
			
			// 检查是否有当前道具
			if (!cell.Mediator.HasCurrentItem())
				return false;
			
			// 获取当前道具
			var currentItem = cell.CurrentItem;
			if (currentItem == null)
				return false;
			
			// 检查当前道具是否是Lightball (ItemType = 5)
			if (currentItem.ItemType != ItemType.Lightball)
				return false;
			
			// 检查是否有下方道具
			if (!cell.StaticMediator.HasBelowItem())
				return false;
			
			// 获取最顶层的下方道具
			var topMostBelowItem = cell.StaticMediator.GetTopMostBelowItem();
			if (topMostBelowItem == null)
				return true;
			
			// 检查下方道具是否是JellyItem
			if (topMostBelowItem is Royal.Scenes.Game.Mechanics.Items.StaticItems.JellyItem.JellyItemModel jellyItem)
			{
				// 如果是JellyItem，检查其IsLogicActive状态
				return jellyItem.IsLogicActive();
			}
			
			// 如果不是JellyItem，返回true（表示有可目标的下方道具）
			return true;
		}

		// Token: 0x0600E05C RID: 57436 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E05C")]
		[Address(RVA = "0x1AC7404", Offset = "0x1AC7404", VA = "0x1AC7404")]
		private void ResetPerformanceData()
		{
			// 空函数 - 可能用于性能数据重置，但当前实现为空
		}

		// Token: 0x0600E05D RID: 57437 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E05D")]
		[Address(RVA = "0x1AC0600", Offset = "0x1AC0600", VA = "0x1AC0600")]
		private void BeginProfile()
		{
			// 空函数 - 可能用于开始性能分析，但当前实现为空
		}

		// Token: 0x0600E05E RID: 57438 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E05E")]
		[Address(RVA = "0x1AC0880", Offset = "0x1AC0880", VA = "0x1AC0880")]
		private void EndProfile()
		{
			// 空函数 - 可能用于结束性能分析，但当前实现为空
		}

		// Token: 0x0600E05F RID: 57439 RVA: 0x00002053 File Offset: 0x00000253
		[Token(Token = "0x600E05F")]
		[Address(RVA = "0x1AC7408", Offset = "0x1AC7408", VA = "0x1AC7408")]
		public void AddExploderMetrics(List<object> metrics)
		{
			// 空函数 
		}

		// Token: 0x0400CE7F RID: 52863
		[Token(Token = "0x400CE7F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x0")]
		private static int PropellerSortingOffset;

		// Token: 0x0400CE80 RID: 52864
		[Token(Token = "0x400CE80")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x10")]
		private readonly List<int> rocketScoreIndexList;

		// Token: 0x0400CE81 RID: 52865
		[Token(Token = "0x400CE81")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x18")]
		private readonly List<CellPoint> areaScorePointList;

		// Token: 0x0400CE82 RID: 52866
		[Token(Token = "0x400CE82")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x20")]
		private CellGridManager gridManager;

		// Token: 0x0400CE83 RID: 52867
		[Token(Token = "0x400CE83")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x28")]
		private LevelRandomManager randomManager;

		// Token: 0x0400CE84 RID: 52868
		[Token(Token = "0x400CE84")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x30")]
		private GoalManager goalManager;

		// Token: 0x0400CE85 RID: 52869
		[Token(Token = "0x400CE85")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x38")]
		private LevelManager levelManager;

		// Token: 0x0400CE86 RID: 52870
		[Token(Token = "0x400CE86")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x40")]
		private GridIterator iterator;

		// Token: 0x0400CE87 RID: 52871
		[Token(Token = "0x400CE87")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x58")]
		private readonly SortedDictionary<int, List<CellModel>> scores;

		// Token: 0x0400CE88 RID: 52872
		[Token(Token = "0x400CE88")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x60")]
		private WinCalculationData winCalculationData;

		// Token: 0x0400CE89 RID: 52873
		[Token(Token = "0x400CE89")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x68")]
		private WaterWinCalculationData waterWinCalculationData;

		// Token: 0x0400CE8A RID: 52874
		[Token(Token = "0x400CE8A")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x70")]
		private readonly WinCalculationData originalWinCalculationData;

		// Token: 0x0400CE8B RID: 52875
		[Token(Token = "0x400CE8B")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x78")]
		private readonly List<int> winningColumnsOrRows;

		// Token: 0x0400CE8C RID: 52876
		[Token(Token = "0x400CE8C")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x80")]
		private int[] columnScores;

		// Token: 0x0400CE8D RID: 52877
		[Token(Token = "0x400CE8D")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x88")]
		private int[] rowScores;

		// Token: 0x0400CE8E RID: 52878
		[Token(Token = "0x400CE8E")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x90")]
		private int[] goalDependentCounts;

		// Token: 0x0400CE8F RID: 52879
		[Token(Token = "0x400CE8F")]
		[Il2CppDummyDll.FieldOffset(Offset = "0x98")]
		private bool[] hasAddedPositiveColumnScore;

		// Token: 0x0400CE90 RID: 52880
		[Token(Token = "0x400CE90")]
		[Il2CppDummyDll.FieldOffset(Offset = "0xA0")]
		private bool[] hasAddedPositiveRowScore;

		// Token: 0x0400CE91 RID: 52881
		[Token(Token = "0x400CE91")]
		[Il2CppDummyDll.FieldOffset(Offset = "0xA8")]
		private bool isWaterLevel;

		// Token: 0x0400CE92 RID: 52882
		[Token(Token = "0x400CE92")]
		[Il2CppDummyDll.FieldOffset(Offset = "0xB0")]
		private readonly bool[] clearedCellsAfterExploderHit;
	}
}
