// Automatic generate by PacketGenerator.
using System;
using System.Collections.Generic;

namespace DragonMarble.Message
{
public enum GameMessageType
{
	OrderCardSelect,
	RollMoveDiceResult,
	RollMoveDice,
	InitializeGame,
	InitailizePlayer,
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
		case GameMessageType.OrderCardSelect:
		case GameMessageType.RollMoveDiceResult:
		case GameMessageType.RollMoveDice:
		case GameMessageType.InitializeGame:
		case GameMessageType.InitailizePlayer:
		message = new InitailizePlayerGameMessage();
		break;
	}
	return message;
}
}
// 선 뽑기	
public class OrderCardSelectGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OrderCardSelect;}}
	public Guid From;
	public Guid To;
	public Int16 SelectedCardNumber;
	public Boolean Result;
	public Int32 NumberOfPlayers;
	public List<Int16> OrderCardSelectState;

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
		BitConverter.GetBytes(Result)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		BitConverter.GetBytes(NumberOfPlayers)
		.CopyTo(bytes,index);
		index += sizeof(Int32);
	for (int i = 0; i < NumberOfPlayers ; i++ )
	{
		BitConverter.GetBytes(OrderCardSelectState[i])
		.CopyTo(bytes,index);
		index += sizeof(Int16);
	}
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 10;
		From = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		To = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		SelectedCardNumber = BitConverter.ToInt16(bytes,index);
		index += sizeof(Int16);
		Result = BitConverter.ToBoolean(bytes,index);
		index += sizeof(Boolean);
		NumberOfPlayers = BitConverter.ToInt32(bytes,index);
		index += sizeof(Int32);
		OrderCardSelectState = new List<Int16>();
	OrderCardSelectState = new List<Int16>();
	for (int i = 0; i < NumberOfPlayers ; i++ )
	{
		Int16 targetOrderCardSelectState = BitConverter.ToInt16(bytes, index);
		OrderCardSelectState.Add(targetOrderCardSelectState);
	}
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16)+(sizeof(Int16))+(sizeof(Boolean))+(sizeof(Int32))+(NumberOfPlayers*(sizeof(Int16))));
	}
}
}

// 서버에서 주사위 굴리기 결과	
public class RollMoveDiceResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDiceResult;}}
	public Guid From;
	public Guid To;
	public List<Int32> Dices;

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
		index += sizeof(Int32);
		BitConverter.GetBytes(Dices[1])
		.CopyTo(bytes,index);
		index += sizeof(Int32);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 10;
		From = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		To = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		Dices = new List<Int32>();
		Dices.Add(BitConverter.ToInt32(bytes,index));
		index += sizeof(Int32);
		Dices.Add(BitConverter.ToInt32(bytes,index));
		index += sizeof(Int32);
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16)+(2*(sizeof(Int32))));
	}
}
}

// 클라이언트에서 이동 주사위 굴리기	
public class RollMoveDiceGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDice;}}
	public Guid From;
	public Guid To;
	public Int32 Pressed;
	public Boolean Odd;
	public Boolean Even;

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
		index += sizeof(Int32);
		BitConverter.GetBytes(Odd)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
		BitConverter.GetBytes(Even)
		.CopyTo(bytes,index);
		index += sizeof(Boolean);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 10;
		From = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		To = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		Pressed = BitConverter.ToInt32(bytes,index);
		index += sizeof(Int32);
		Odd = BitConverter.ToBoolean(bytes,index);
		index += sizeof(Boolean);
		Even = BitConverter.ToBoolean(bytes,index);
		index += sizeof(Boolean);
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16)+(sizeof(Int32))+(sizeof(Boolean))+(sizeof(Boolean)));
	}
}
}

// 게임 초기화 정보	
public class InitializeGameGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitializeGame;}}
	public Guid From;
	public Guid To;
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
		BitConverter.GetBytes(Units[i].Capital)
		.CopyTo(bytes,index);
		index += sizeof(Int32);
		BitConverter.GetBytes((Int32)Units[i].teamColor)
		.CopyTo(bytes,index);
		index += sizeof(StageUnitInfo.TEAM_COLOR);
	}
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 10;
		From = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		To = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		FeeBoostedTiles = new List<Int16>();
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes,index));
		index += sizeof(Int16);
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes,index));
		index += sizeof(Int16);
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes,index));
		index += sizeof(Int16);
		FeeBoostedTiles.Add(BitConverter.ToInt16(bytes,index));
		index += sizeof(Int16);
		NumberOfPlayers = BitConverter.ToInt16(bytes,index);
		index += sizeof(Int16);
		Units = new List<StageUnitInfo>();
	Units = new List<StageUnitInfo>();
	for (int i = 0; i < NumberOfPlayers ; i++ )
	{
		StageUnitInfo targetUnits = new StageUnitInfo();
		targetUnits.gold = BitConverter.ToInt32(bytes, index);
		index += 4 * sizeof(Int32);
		targetUnits.Order = BitConverter.ToInt32(bytes, index);
		index += 4 * sizeof(Int32);
		targetUnits.Capital = BitConverter.ToInt32(bytes, index);
		index += 4 * sizeof(Int32);
		targetUnits.teamColor = (StageUnitInfo.TEAM_COLOR)BitConverter.ToInt32(bytes, index);
		index += 4 * sizeof(Int32);
		Units.Add(targetUnits);
	}
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16)+(4*(sizeof(Int16)))+(sizeof(Int16))+(NumberOfPlayers*(4 * sizeof(Int32))));
	}
}
}

// 플레이어 초기화	
public class InitailizePlayerGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitailizePlayer;}}
	public Guid From;
	public Guid To;

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
		int index = 10;
		From = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
		To = new Guid(new ArraySegment<Byte>(bytes, index,16).Array);
		index += 16;
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16));
	}
}
}

}