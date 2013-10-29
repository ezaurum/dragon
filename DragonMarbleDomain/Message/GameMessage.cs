// Automatic generate by PacketGenerator.
using System;
using System.Collections.Generic;

namespace DragonMarble.Message
{
public enum GameMessageType
{
	ActionResultCopy,
	ActivateTurn,
	BuyLand,
	BuyLandRequest,
	GameResult,
	InitializeGame,
	InitializePlayer,
	LoanMoney,
	NeedMoneyRequest,
	NothingToDo,
	OrderCardResult,
	OrderCardSelect,
	PayFee,
	PrisonAction,
	PrisonActionResult,
	RollMoveDice,
	RollMoveDiceResult,
	SellLands,
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
		case GameMessageType.BuyLand:
		message = new BuyLandGameMessage();
		break;
		case GameMessageType.BuyLandRequest:
		message = new BuyLandRequestGameMessage();
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
		case GameMessageType.LoanMoney:
		message = new LoanMoneyGameMessage();
		break;
		case GameMessageType.NeedMoneyRequest:
		message = new NeedMoneyRequestGameMessage();
		break;
		case GameMessageType.NothingToDo:
		message = new NothingToDoGameMessage();
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
		case GameMessageType.RollMoveDice:
		message = new RollMoveDiceGameMessage();
		break;
		case GameMessageType.RollMoveDiceResult:
		message = new RollMoveDiceResultGameMessage();
		break;
		case GameMessageType.SellLands:
		message = new SellLandsGameMessage();
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

// game result	
public class GameResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.GameResult;}}
	public Guid LoseUnit { get; set;}

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
		LoseUnit.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempLoseUnit = new byte[16];
		Buffer.BlockCopy(bytes, index, tempLoseUnit, 0,16);
		LoseUnit = new Guid(tempLoseUnit);
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
		index += sizeof(Int32);
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
		tempUnits.gold = BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
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
			return (Int16)(2+sizeof(GameMessageType)+4*(sizeof(Int16))+sizeof(Int16)+NumberOfPlayers*(sizeof(Int32)+sizeof(Int32)+sizeof(StageUnitInfo.UNIT_COLOR)+sizeof(StageUnitInfo.TEAM_GROUP)+sizeof(StageUnitInfo.ControlModeType)+16));
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

// loan money from bank	
public class LoanMoneyGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.LoanMoney;}}
	public Int32 LoanMoney;
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
		index += sizeof(Int32);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		LoanMoney = BitConverter.ToInt32(bytes, index);
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

// request loan money	
public class NeedMoneyRequestGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.NeedMoneyRequest;}}
	public Int32 NeedMoney;
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
		index += sizeof(Int32);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		NeedMoney = BitConverter.ToInt32(bytes, index);
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
	public Int32 Fee;
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
		index += sizeof(Int32);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		Fee = BitConverter.ToInt32(bytes, index);
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