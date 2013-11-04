// Automatic generate by PacketGenerator.
using System;
using System.Collections.Generic;

namespace DragonMarble.Message
{
public enum GameMessageType
{
	ActionResultCopy,
	ActivateTurn,
	Bankrupt,
	BuyItemInGame,
	BuyLand,
	BuyLandRequest,
	ChanceCardBuff,
	ChanceCardCoupon,
	ChanceCardGoTo,
	ChanceCardOrder,
	EveryoneIsReady,
	ExitWaitingRoom,
	GambleBatting,
	GambleChoice,
	GambleResult,
	GameResult,
	InitializeGame,
	InitializePlayer,
	InitializeWaitingRoom,
	LoanMoney,
	NeedMoneyRequest,
	NewPlayerJoin,
	NothingToDo,
	OpenChanceCard,
	OpenOlympic,
	OrderCardResult,
	OrderCardSelect,
	PayFee,
	PrisonAction,
	PrisonActionResult,
	ReadyState,
	RollMoveDice,
	RollMoveDiceResult,
	RoomOwner,
	SellLands,
	Session,
	StartGame,
	Takeover,
	TakeoverRequest,
	TravelAction,
	UseCoupon,
	UseCouponRequest,
}
public static class GameMessageFactory
{
	public static IDragonMarbleGameMessage GetGameMessage(byte[] bytes)
	{
		GameMessageType messageType = (GameMessageType) BitConverter.ToInt32(bytes, 2);
		IDragonMarbleGameMessage message = GetGameMessage(messageType);
		message.FromByteArray(bytes);
		return message;
	}
	public static IDragonMarbleGameMessage GetGameMessage(GameMessageType messageType)
	{
		IDragonMarbleGameMessage message = null;
		switch (messageType)
		{
		case GameMessageType.ActionResultCopy:
		message = new ActionResultCopyGameMessage();
		break;
		case GameMessageType.ActivateTurn:
		message = new ActivateTurnGameMessage();
		break;
		case GameMessageType.Bankrupt:
		message = new BankruptGameMessage();
		break;
		case GameMessageType.BuyItemInGame:
		message = new BuyItemInGameGameMessage();
		break;
		case GameMessageType.BuyLand:
		message = new BuyLandGameMessage();
		break;
		case GameMessageType.BuyLandRequest:
		message = new BuyLandRequestGameMessage();
		break;
		case GameMessageType.ChanceCardBuff:
		message = new ChanceCardBuffGameMessage();
		break;
		case GameMessageType.ChanceCardCoupon:
		message = new ChanceCardCouponGameMessage();
		break;
		case GameMessageType.ChanceCardGoTo:
		message = new ChanceCardGoToGameMessage();
		break;
		case GameMessageType.ChanceCardOrder:
		message = new ChanceCardOrderGameMessage();
		break;
		case GameMessageType.EveryoneIsReady:
		message = new EveryoneIsReadyGameMessage();
		break;
		case GameMessageType.ExitWaitingRoom:
		message = new ExitWaitingRoomGameMessage();
		break;
		case GameMessageType.GambleBatting:
		message = new GambleBattingGameMessage();
		break;
		case GameMessageType.GambleChoice:
		message = new GambleChoiceGameMessage();
		break;
		case GameMessageType.GambleResult:
		message = new GambleResultGameMessage();
		break;
		case GameMessageType.GameResult:
		message = new GameResultGameMessage();
		break;
		case GameMessageType.InitializeGame:
		message = new InitializeGameGameMessage();
		break;
		case GameMessageType.InitializePlayer:
		message = new InitializePlayerGameMessage();
		break;
		case GameMessageType.InitializeWaitingRoom:
		message = new InitializeWaitingRoomGameMessage();
		break;
		case GameMessageType.LoanMoney:
		message = new LoanMoneyGameMessage();
		break;
		case GameMessageType.NeedMoneyRequest:
		message = new NeedMoneyRequestGameMessage();
		break;
		case GameMessageType.NewPlayerJoin:
		message = new NewPlayerJoinGameMessage();
		break;
		case GameMessageType.NothingToDo:
		message = new NothingToDoGameMessage();
		break;
		case GameMessageType.OpenChanceCard:
		message = new OpenChanceCardGameMessage();
		break;
		case GameMessageType.OpenOlympic:
		message = new OpenOlympicGameMessage();
		break;
		case GameMessageType.OrderCardResult:
		message = new OrderCardResultGameMessage();
		break;
		case GameMessageType.OrderCardSelect:
		message = new OrderCardSelectGameMessage();
		break;
		case GameMessageType.PayFee:
		message = new PayFeeGameMessage();
		break;
		case GameMessageType.PrisonAction:
		message = new PrisonActionGameMessage();
		break;
		case GameMessageType.PrisonActionResult:
		message = new PrisonActionResultGameMessage();
		break;
		case GameMessageType.ReadyState:
		message = new ReadyStateGameMessage();
		break;
		case GameMessageType.RollMoveDice:
		message = new RollMoveDiceGameMessage();
		break;
		case GameMessageType.RollMoveDiceResult:
		message = new RollMoveDiceResultGameMessage();
		break;
		case GameMessageType.RoomOwner:
		message = new RoomOwnerGameMessage();
		break;
		case GameMessageType.SellLands:
		message = new SellLandsGameMessage();
		break;
		case GameMessageType.Session:
		message = new SessionGameMessage();
		break;
		case GameMessageType.StartGame:
		message = new StartGameGameMessage();
		break;
		case GameMessageType.Takeover:
		message = new TakeoverGameMessage();
		break;
		case GameMessageType.TakeoverRequest:
		message = new TakeoverRequestGameMessage();
		break;
		case GameMessageType.TravelAction:
		message = new TravelActionGameMessage();
		break;
		case GameMessageType.UseCoupon:
		message = new UseCouponGameMessage();
		break;
		case GameMessageType.UseCouponRequest:
		message = new UseCouponRequestGameMessage();
		break;
		}
		return message;
	}
}
// Client received Player's action result signal	
public class ActionResultCopyGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ActionResultCopy;}}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType));
		}
	}
}

// Activate player's turn	
public class ActivateTurnGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ActivateTurn;}}
	public Guid TurnOwner { get; set;}
	public Int32 ResponseLimit;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		TurnOwner.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		BitConverter.GetBytes(ResponseLimit)
		.CopyTo(bytes,index);
		index += sizeof(Int32);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempTurnOwner = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTurnOwner, 0,16);
		TurnOwner = new Guid(tempTurnOwner);
		index += 16;
		ResponseLimit = BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16+sizeof(Int32));
		}
	}
}

// Bankrupt	
public class BankruptGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.Bankrupt;}}
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

// Buy a game item in waiting room or game playing	
public class BuyItemInGameGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.BuyItemInGame;}}
	public Guid ItemId { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		ItemId.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempItemId = new byte[16];
		Buffer.BlockCopy(bytes, index, tempItemId, 0,16);
		ItemId = new Guid(tempItemId);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

// buy lands	
public class BuyLandGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.BuyLand;}}
	public Char TileIndex;
	public Char Buildings;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(TileIndex)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		BitConverter.GetBytes(Buildings)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		TileIndex = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		Buildings = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+sizeof(Char)+16);
		}
	}
}

// buy lands available	
public class BuyLandRequestGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.BuyLandRequest;}}
	public Int32 ResponseLimit;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(ResponseLimit)
		.CopyTo(bytes,index);
		index += sizeof(Int32);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		ResponseLimit = BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int32)+16);
		}
	}
}

// chance card buff	
public class ChanceCardBuffGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ChanceCardBuff;}}
	public Char CardId;
	public Char SelectTile;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(CardId)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		BitConverter.GetBytes(SelectTile)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		CardId = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		SelectTile = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+sizeof(Char)+16);
		}
	}
}

// save the chance card	
public class ChanceCardCouponGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ChanceCardCoupon;}}
	public Char CardId;
	public Boolean Save;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(CardId)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		BitConverter.GetBytes(Save)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		CardId = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		Save = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+sizeof(Boolean)+16);
		}
	}
}

// chqnce card move	
public class ChanceCardGoToGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ChanceCardGoTo;}}
	public Char CardId;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(CardId)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		CardId = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+16);
		}
	}
}

// chance card order	
public class ChanceCardOrderGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ChanceCardOrder;}}
	public Char CardId;
	public Char Value1;
	public Char Value2;
	public Guid Actor { get; set;}
	public Guid Target { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(CardId)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		BitConverter.GetBytes(Value1)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		BitConverter.GetBytes(Value2)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		Target.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		CardId = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		Value1 = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		Value2 = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
		byte[] tempTarget = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTarget, 0,16);
		Target = new Guid(tempTarget);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+sizeof(Char)+sizeof(Char)+16+16);
		}
	}
}

// Everyone is ready for game start	
public class EveryoneIsReadyGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.EveryoneIsReady;}}
	public Int32 ResponseLimit;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(ResponseLimit)
		.CopyTo(bytes,index);
		index += sizeof(Int32);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		ResponseLimit = BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int32));
		}
	}
}

// Exit waiting room	
public class ExitWaitingRoomGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ExitWaitingRoom;}}
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

// batting	
public class GambleBattingGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.GambleBatting;}}
	public Char BattingPrice;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(BattingPrice)
		.CopyTo(bytes,index);
		index += sizeof(Char);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		BattingPrice = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char));
		}
	}
}

// choice	
public class GambleChoiceGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.GambleChoice;}}
	public Char Choice;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(Choice)
		.CopyTo(bytes,index);
		index += sizeof(Char);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		Choice = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char));
		}
	}
}

// gamble result	
public class GambleResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.GambleResult;}}
	public Char WinCount;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(WinCount)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		WinCount = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+16);
		}
	}
}

// game result	
public class GameResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.GameResult;}}
	public StageUnitInfo.TEAM_GROUP WinTeam;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes((Int32)WinTeam)
		.CopyTo(bytes,index);
		index += sizeof(StageUnitInfo.TEAM_GROUP);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		WinTeam = (StageUnitInfo.TEAM_GROUP)BitConverter.ToInt32(bytes, index);
		index += sizeof(StageUnitInfo.TEAM_GROUP);
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(StageUnitInfo.TEAM_GROUP));
		}
	}
}

// 게임 초기화 정보	
public class InitializeGameGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitializeGame;}}
	public List<Int16> FeeBoostedTiles;
	public Int16 NumberOfPlayers;
	public List<StageUnitInfo> Units;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(FeeBoostedTiles[0])
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes(FeeBoostedTiles[1])
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes(FeeBoostedTiles[2])
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes(FeeBoostedTiles[3])
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes(NumberOfPlayers)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
	for (int i = 0; i < NumberOfPlayers ; i++ )
	{
		BitConverter.GetBytes(Units[i].gold)
		.CopyTo(bytes,index);
		index += sizeof(Int64);
		BitConverter.GetBytes(Units[i].Order)
		.CopyTo(bytes,index);
		index += sizeof(Int32);
		BitConverter.GetBytes((Int32)Units[i].unitColor)
		.CopyTo(bytes,index);
		index += sizeof(StageUnitInfo.UNIT_COLOR);
		BitConverter.GetBytes((Int32)Units[i].teamGroup)
		.CopyTo(bytes,index);
		index += sizeof(StageUnitInfo.TEAM_GROUP);
		BitConverter.GetBytes((Int32)Units[i].ControlMode)
		.CopyTo(bytes,index);
		index += sizeof(StageUnitInfo.ControlModeType);
		Units[i].Id.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	}
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		FeeBoostedTiles = new List<Int16>();
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes, index));
		index += sizeof(Int16);
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes, index));
		index += sizeof(Int16);
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes, index));
		index += sizeof(Int16);
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes, index));
		index += sizeof(Int16);
		NumberOfPlayers = BitConverter.ToInt16(bytes, index);
		index += sizeof(Int16);
		Units = new List<StageUnitInfo>();
		for(int i = 0; i < NumberOfPlayers; i++)
		{
		StageUnitInfo tempUnits = new StageUnitInfo();
		tempUnits.gold = BitConverter.ToInt64(bytes, index);
		index += sizeof(Int64);
		tempUnits.Order = BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
		tempUnits.unitColor = (StageUnitInfo.UNIT_COLOR)BitConverter.ToInt32(bytes, index);
		index += sizeof(StageUnitInfo.UNIT_COLOR);
		tempUnits.teamGroup = (StageUnitInfo.TEAM_GROUP)BitConverter.ToInt32(bytes, index);
		index += sizeof(StageUnitInfo.TEAM_GROUP);
		tempUnits.ControlMode = (StageUnitInfo.ControlModeType)BitConverter.ToInt32(bytes, index);
		index += sizeof(StageUnitInfo.ControlModeType);
		byte[] temptempUnits_Id = new byte[16];
		Buffer.BlockCopy(bytes, index, temptempUnits_Id, 0,16);
		tempUnits.Id = new Guid(temptempUnits_Id);
		index += 16;
		Units.Add(tempUnits);
		}
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+4*(sizeof(Int16))+sizeof(Int16)+NumberOfPlayers*(sizeof(Int64)+sizeof(Int32)+sizeof(StageUnitInfo.UNIT_COLOR)+sizeof(StageUnitInfo.TEAM_GROUP)+sizeof(StageUnitInfo.ControlModeType)+16));
		}
	}
}

// 플레이어 초기화	
public class InitializePlayerGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitializePlayer;}}
	public Guid Server { get; set;}
	public Guid PlayerId { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		Server.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		PlayerId.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempServer = new byte[16];
		Buffer.BlockCopy(bytes, index, tempServer, 0,16);
		Server = new Guid(tempServer);
		index += 16;
		byte[] tempPlayerId = new byte[16];
		Buffer.BlockCopy(bytes, index, tempPlayerId, 0,16);
		PlayerId = new Guid(tempPlayerId);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16+16);
		}
	}
}

// 게임 대기방 초기화 정보	
public class InitializeWaitingRoomGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitializeWaitingRoom;}}
	public Int16 BoardType;
	public Int16 NumberOfPlayers;
	public Int16 CurrentNumberOfPlayers;
	public List<StageUnitInfo> Units;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(BoardType)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes(NumberOfPlayers)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes(CurrentNumberOfPlayers)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
	for (int i = 0; i < CurrentNumberOfPlayers ; i++ )
	{
		BitConverter.GetBytes((Int32)Units[i].teamGroup)
		.CopyTo(bytes,index);
		index += sizeof(StageUnitInfo.TEAM_GROUP);
		Units[i].Id.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		BitConverter.GetBytes(Units[i].IsReady)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
	}
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		BoardType = BitConverter.ToInt16(bytes, index);
		index += sizeof(Int16);
		NumberOfPlayers = BitConverter.ToInt16(bytes, index);
		index += sizeof(Int16);
		CurrentNumberOfPlayers = BitConverter.ToInt16(bytes, index);
		index += sizeof(Int16);
		Units = new List<StageUnitInfo>();
		for(int i = 0; i < CurrentNumberOfPlayers; i++)
		{
		StageUnitInfo tempUnits = new StageUnitInfo();
		tempUnits.teamGroup = (StageUnitInfo.TEAM_GROUP)BitConverter.ToInt32(bytes, index);
		index += sizeof(StageUnitInfo.TEAM_GROUP);
		byte[] temptempUnits_Id = new byte[16];
		Buffer.BlockCopy(bytes, index, temptempUnits_Id, 0,16);
		tempUnits.Id = new Guid(temptempUnits_Id);
		index += 16;
		tempUnits.IsReady = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
		Units.Add(tempUnits);
		}
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int16)+sizeof(Int16)+sizeof(Int16)+CurrentNumberOfPlayers*(sizeof(StageUnitInfo.TEAM_GROUP)+16+sizeof(Boolean)));
		}
	}
}

// loan money from bank	
public class LoanMoneyGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.LoanMoney;}}
	public Int64 LoanMoney;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(LoanMoney)
		.CopyTo(bytes,index);
		index += sizeof(Int64);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		LoanMoney = BitConverter.ToInt64(bytes, index);
		index += sizeof(Int64);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int64)+16);
		}
	}
}

// request loan money	
public class NeedMoneyRequestGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.NeedMoneyRequest;}}
	public Int64 NeedMoney;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(NeedMoney)
		.CopyTo(bytes,index);
		index += sizeof(Int64);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		NeedMoney = BitConverter.ToInt64(bytes, index);
		index += sizeof(Int64);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int64)+16);
		}
	}
}

// 게임 대기방 추가 정보	
public class NewPlayerJoinGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.NewPlayerJoin;}}
	public Guid PlayerId { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		PlayerId.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempPlayerId = new byte[16];
		Buffer.BlockCopy(bytes, index, tempPlayerId, 0,16);
		PlayerId = new Guid(tempPlayerId);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

// 할 수 있는게 없을 때	
public class NothingToDoGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.NothingToDo;}}
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

// open a chance card	
public class OpenChanceCardGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OpenChanceCard;}}
	public Char CardId;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(CardId)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		CardId = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+16);
		}
	}
}

// open olympic	
public class OpenOlympicGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OpenOlympic;}}
	public Char TileIndex;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(TileIndex)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		TileIndex = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+16);
		}
	}
}

// 선 뽑기 결과	
public class OrderCardResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OrderCardResult;}}
	public Int16 FirstCardNumber;
	public Guid FirstPlayerId;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(FirstCardNumber)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		FirstPlayerId.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		FirstCardNumber = BitConverter.ToInt16(bytes, index);
		index += sizeof(Int16);
		byte[] tempFirstPlayerId = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFirstPlayerId, 0,16);
		FirstPlayerId = new Guid(tempFirstPlayerId);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int16)+16);
		}
	}
}

// 선 뽑기	
public class OrderCardSelectGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OrderCardSelect;}}
	public Int16 SelectedCardNumber;
	public Int16 NumberOfPlayers;
	public List<Boolean> OrderCardSelectState;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(SelectedCardNumber)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes(NumberOfPlayers)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
	for (int i = 0; i < NumberOfPlayers ; i++ )
	{
		BitConverter.GetBytes(OrderCardSelectState[i])
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
	}
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		SelectedCardNumber = BitConverter.ToInt16(bytes, index);
		index += sizeof(Int16);
		NumberOfPlayers = BitConverter.ToInt16(bytes, index);
		index += sizeof(Int16);
		OrderCardSelectState = new List<Boolean>();
		for(int i = 0; i < NumberOfPlayers; i++)
		{
		OrderCardSelectState.Add(BitConverter.ToBoolean(bytes, index));
		index += sizeof(Boolean);
		}
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int16)+sizeof(Int16)+NumberOfPlayers*(sizeof(Boolean))+16);
		}
	}
}

// pay fee	
public class PayFeeGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.PayFee;}}
	public Int64 Fee;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(Fee)
		.CopyTo(bytes,index);
		index += sizeof(Int64);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		Fee = BitConverter.ToInt64(bytes, index);
		index += sizeof(Int64);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Int64)+16);
		}
	}
}

// action in Prison	
public class PrisonActionGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.PrisonAction;}}
	public Char ActionIndex;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(ActionIndex)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		ActionIndex = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+16);
		}
	}
}

// action result in Prison	
public class PrisonActionResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.PrisonActionResult;}}
	public Boolean EscapeResult;
	public Char EscapeType;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(EscapeResult)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		BitConverter.GetBytes(EscapeType)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		EscapeResult = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
		EscapeType = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Boolean)+sizeof(Char)+16);
		}
	}
}

// Indicate Ready or not for game Start	
public class ReadyStateGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ReadyState;}}
	public Guid Actor { get; set;}
	public Boolean Ready;

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		BitConverter.GetBytes(Ready)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
		Ready = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16+sizeof(Boolean));
		}
	}
}

// 클라이언트에서 이동 주사위 굴리기	
public class RollMoveDiceGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDice;}}
	public Single Pressed;
	public Boolean Odd;
	public Boolean Even;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(Pressed)
		.CopyTo(bytes,index);
		index += sizeof(Single);
		BitConverter.GetBytes(Odd)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		BitConverter.GetBytes(Even)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		Pressed = BitConverter.ToSingle(bytes, index);
		index += sizeof(Single);
		Odd = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
		Even = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Single)+sizeof(Boolean)+sizeof(Boolean)+16);
		}
	}
}

// 서버에서 주사위 굴리기 결과	
public class RollMoveDiceResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDiceResult;}}
	public List<Char> Dices;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(Dices[0])
		.CopyTo(bytes,index);
		index += sizeof(Char);
		BitConverter.GetBytes(Dices[1])
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		Dices = new List<Char>();
		Dices.Add(BitConverter.ToChar(bytes, index));
		index += sizeof(Char);
		Dices.Add(BitConverter.ToChar(bytes, index));
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+2*(sizeof(Char))+16);
		}
	}
}

// Indicate who is room owner	
public class RoomOwnerGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RoomOwner;}}
	public Guid RoomOwner { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		RoomOwner.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempRoomOwner = new byte[16];
		Buffer.BlockCopy(bytes, index, tempRoomOwner, 0,16);
		RoomOwner = new Guid(tempRoomOwner);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

// sell lands	
public class SellLandsGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.SellLands;}}
	public Char LandCount;
	public List<Char> LandList;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(LandCount)
		.CopyTo(bytes,index);
		index += sizeof(Char);
	for (int i = 0; i < LandCount ; i++ )
	{
		BitConverter.GetBytes(LandList[i])
		.CopyTo(bytes,index);
		index += sizeof(Char);
	}
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		LandCount = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		LandList = new List<Char>();
		for(int i = 0; i < LandCount; i++)
		{
		LandList.Add(BitConverter.ToChar(bytes, index));
		index += sizeof(Char);
		}
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+LandCount*(sizeof(Char))+16);
		}
	}
}

// 세션 키 발급/조회	
public class SessionGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.Session;}}
	public Guid SessionKey { get; set;}
	public Guid GameAccountId { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		SessionKey.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		GameAccountId.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempSessionKey = new byte[16];
		Buffer.BlockCopy(bytes, index, tempSessionKey, 0,16);
		SessionKey = new Guid(tempSessionKey);
		index += 16;
		byte[] tempGameAccountId = new byte[16];
		Buffer.BlockCopy(bytes, index, tempGameAccountId, 0,16);
		GameAccountId = new Guid(tempGameAccountId);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16+16);
		}
	}
}

// start game	
public class StartGameGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.StartGame;}}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType));
		}
	}
}

// takeover land	
public class TakeoverGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.Takeover;}}
	public Boolean Takeover;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(Takeover)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		Takeover = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Boolean)+16);
		}
	}
}

// takeover land available	
public class TakeoverRequestGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.TakeoverRequest;}}
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

// action in Travel	
public class TravelActionGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.TravelAction;}}
	public Char TileIndex;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(TileIndex)
		.CopyTo(bytes,index);
		index += sizeof(Char);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		TileIndex = BitConverter.ToChar(bytes, index);
		index += sizeof(Char);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Char)+16);
		}
	}
}

// use chance chance coupon	
public class UseCouponGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.UseCoupon;}}
	public Boolean Use;
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		BitConverter.GetBytes(Use)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		Use = BitConverter.ToBoolean(bytes, index);
		index += sizeof(Boolean);
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+sizeof(Boolean)+16);
		}
	}
}

// request use chance chance coupon	
public class UseCouponRequestGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.UseCouponRequest;}}
	public Guid Actor { get; set;}

	public byte[] ToByteArray()
	{
		byte[] bytes = new byte[Length];
		int index = 0;
		BitConverter.GetBytes(Length)
		.CopyTo(bytes,index);
		index += sizeof(Int16);
		BitConverter.GetBytes((Int32)MessageType)
		.CopyTo(bytes,index);
		index += sizeof(GameMessageType);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempActor = new byte[16];
		Buffer.BlockCopy(bytes, index, tempActor, 0,16);
		Actor = new Guid(tempActor);
		index += 16;
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16);
		}
	}
}

}