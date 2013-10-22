// Automatic generate by PacketGenerator.
using System;
using System.Collections.Generic;
using Dragon.Interfaces;

namespace DragonMarble.Message
{
public enum GameMessageType
{
	ActivateTurn,
	OrderCardSelect,
	InitializeGame,
	BuyLandRequest,
	BuyLand,
	InitializePlayer,
	RollMoveDiceResult,
	OrderCardResult,
	ForceMoveToPrison,
	RollMoveDice,
	NothingToDo,
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
		case GameMessageType.ActivateTurn:
		message = new ActivateTurnGameMessage();
		break;
		case GameMessageType.OrderCardSelect:
		message = new OrderCardSelectGameMessage();
		break;
		case GameMessageType.InitializeGame:
		message = new InitializeGameGameMessage();
		break;
		case GameMessageType.BuyLandRequest:
		message = new BuyLandRequestGameMessage();
		break;
		case GameMessageType.BuyLand:
		message = new BuyLandGameMessage();
		break;
		case GameMessageType.InitializePlayer:
		message = new InitializePlayerGameMessage();
		break;
		case GameMessageType.RollMoveDiceResult:
		message = new RollMoveDiceResultGameMessage();
		break;
		case GameMessageType.OrderCardResult:
		message = new OrderCardResultGameMessage();
		break;
		case GameMessageType.ForceMoveToPrison:
		message = new ForceMoveToPrisonGameMessage();
		break;
		case GameMessageType.RollMoveDice:
		message = new RollMoveDiceGameMessage();
		break;
		case GameMessageType.NothingToDo:
		message = new NothingToDoGameMessage();
		break;
		}
		return message;
	}
}
// Activate player's turn	
public class ActivateTurnGameMessage : IDragonMarbleGameMessage, IGameAction	
{
	public GameMessageType MessageType {get{return GameMessageType.ActivateTurn;}}
	public Guid TurnOwner { get; set;}
	public Int64 ResponseLimit;

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
		index += sizeof(Int64);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempTurnOwner = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTurnOwner, 0,16);
		TurnOwner = new Guid(tempTurnOwner);
		index += 16;
		ResponseLimit = BitConverter.ToInt64(bytes, index);
		index += sizeof(Int64);
}

	public Int16 Length
	{
		get
		{
			return (Int16)(2+sizeof(GameMessageType)+16+sizeof(Int64));
		}
	}
}

// 선 뽑기	
public class OrderCardSelectGameMessage : IDragonMarbleGameMessage, IGameAction	
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

// 게임 초기화 정보	
public class InitializeGameGameMessage : IDragonMarbleGameMessage, IGameAction	
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

// buy lands available	
public class BuyLandRequestGameMessage : IDragonMarbleGameMessage, IGameAction	
{
	public GameMessageType MessageType {get{return GameMessageType.BuyLandRequest;}}
	public Int64 ResponseLimit;
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
		index += sizeof(Int64);
		Actor.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		ResponseLimit = BitConverter.ToInt64(bytes, index);
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

// buy lands	
public class BuyLandGameMessage : IDragonMarbleGameMessage, IGameAction	
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

// 플레이어 초기화	
public class InitializePlayerGameMessage : IDragonMarbleGameMessage, IGameAction	
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

// 서버에서 주사위 굴리기 결과	
public class RollMoveDiceResultGameMessage : IDragonMarbleGameMessage, IGameAction	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDiceResult;}}
	public List<Char> Dices;
	public Char RollCount;
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
		BitConverter.GetBytes(RollCount)
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
		RollCount = BitConverter.ToChar(bytes, index);
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
			return (Int16)(2+sizeof(GameMessageType)+2*(sizeof(Char))+sizeof(Char)+16);
		}
	}
}

// 선 뽑기 결과	
public class OrderCardResultGameMessage : IDragonMarbleGameMessage, IGameAction	
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

// 3더블	
public class ForceMoveToPrisonGameMessage : IDragonMarbleGameMessage, IGameAction	
{
	public GameMessageType MessageType {get{return GameMessageType.ForceMoveToPrison;}}

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

// 클라이언트에서 이동 주사위 굴리기	
public class RollMoveDiceGameMessage : IDragonMarbleGameMessage, IGameAction	
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

// 할 수 있는게 없을 때	
public class NothingToDoGameMessage : IDragonMarbleGameMessage, IGameAction	
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

}