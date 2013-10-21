// Automatic generate by PacketGenerator.
using System;
using System.Collections.Generic;

namespace DragonMarble.Message
{
public enum GameMessageType
{
	OrderCardResult,
	OrderCardSelect,
	RollMoveDiceResult,
	ActivateTurn,
	InitializeGame,
	InitializePlayer,
	RollMoveDice,
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
		case GameMessageType.OrderCardResult:
		message = new OrderCardResultGameMessage();
		break;
		case GameMessageType.OrderCardSelect:
		message = new OrderCardSelectGameMessage();
		break;
		case GameMessageType.RollMoveDiceResult:
		message = new RollMoveDiceResultGameMessage();
		break;
		case GameMessageType.ActivateTurn:
		message = new ActivateTurnGameMessage();
		break;
		case GameMessageType.InitializeGame:
		message = new InitializeGameGameMessage();
		break;
		case GameMessageType.InitializePlayer:
		message = new InitializePlayerGameMessage();
		break;
		case GameMessageType.RollMoveDice:
		message = new RollMoveDiceGameMessage();
		break;
		}
		return message;
	}
}
// 선 뽑기 결과	
public class OrderCardResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OrderCardResult;}}
	public Guid From { get; set;}
	public Guid To { get; set;}
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
		From.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		To.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
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
		byte[] tempFrom = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFrom, 0,16);
		From = new Guid(tempFrom);
		index += 16;
		byte[] tempTo = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTo, 0,16);
		To = new Guid(tempTo);
		index += 16;
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
			return (Int16)(2+sizeof(GameMessageType)+16+16+sizeof(Int16)+16);
		}
	}
}

// 선 뽑기	
public class OrderCardSelectGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OrderCardSelect;}}
	public Guid From { get; set;}
	public Guid To { get; set;}
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
		From.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		To.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
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
		byte[] tempFrom = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFrom, 0,16);
		From = new Guid(tempFrom);
		index += 16;
		byte[] tempTo = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTo, 0,16);
		To = new Guid(tempTo);
		index += 16;
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
			return (Int16)(2+sizeof(GameMessageType)+16+16+sizeof(Int16)+sizeof(Int16)+NumberOfPlayers*(sizeof(Boolean))+16);
		}
	}
}

// 서버에서 주사위 굴리기 결과	
public class RollMoveDiceResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDiceResult;}}
	public Guid From { get; set;}
	public Guid To { get; set;}
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
		From.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		To.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
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
		byte[] tempFrom = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFrom, 0,16);
		From = new Guid(tempFrom);
		index += 16;
		byte[] tempTo = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTo, 0,16);
		To = new Guid(tempTo);
		index += 16;
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
			return (Int16)(2+sizeof(GameMessageType)+16+16+2*(sizeof(Char))+sizeof(Char)+16);
		}
	}
}

// Activate player's turn	
public class ActivateTurnGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.ActivateTurn;}}
	public Guid From { get; set;}
	public Guid To { get; set;}
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
		From.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		To.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
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
		byte[] tempFrom = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFrom, 0,16);
		From = new Guid(tempFrom);
		index += 16;
		byte[] tempTo = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTo, 0,16);
		To = new Guid(tempTo);
		index += 16;
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
			return (Int16)(2+sizeof(GameMessageType)+16+16+16+sizeof(Int64));
		}
	}
}

// 게임 초기화 정보	
public class InitializeGameGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitializeGame;}}
	public Guid From { get; set;}
	public Guid To { get; set;}
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
		From.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		To.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
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
		byte[] tempFrom = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFrom, 0,16);
		From = new Guid(tempFrom);
		index += 16;
		byte[] tempTo = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTo, 0,16);
		To = new Guid(tempTo);
		index += 16;
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
			return (Int16)(2+sizeof(GameMessageType)+16+16+4*(sizeof(Int16))+sizeof(Int16)+NumberOfPlayers*(sizeof(Int32)+sizeof(Int32)+sizeof(StageUnitInfo.UNIT_COLOR)+sizeof(StageUnitInfo.TEAM_GROUP)+sizeof(StageUnitInfo.ControlModeType)+16));
		}
	}
}

// 플레이어 초기화	
public class InitializePlayerGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitializePlayer;}}
	public Guid From { get; set;}
	public Guid To { get; set;}

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
		From.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		To.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
		byte[] tempFrom = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFrom, 0,16);
		From = new Guid(tempFrom);
		index += 16;
		byte[] tempTo = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTo, 0,16);
		To = new Guid(tempTo);
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

// 클라이언트에서 이동 주사위 굴리기	
public class RollMoveDiceGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDice;}}
	public Guid From { get; set;}
	public Guid To { get; set;}
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
		From.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
		To.ToByteArray()
		.CopyTo(bytes,index);
		index += 16;
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
		byte[] tempFrom = new byte[16];
		Buffer.BlockCopy(bytes, index, tempFrom, 0,16);
		From = new Guid(tempFrom);
		index += 16;
		byte[] tempTo = new byte[16];
		Buffer.BlockCopy(bytes, index, tempTo, 0,16);
		To = new Guid(tempTo);
		index += 16;
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
			return (Int16)(2+sizeof(GameMessageType)+16+16+sizeof(Single)+sizeof(Boolean)+sizeof(Boolean)+16);
		}
	}
}

}