// Automatic generate by PacketGenerator.
using System;
using System.Collections.Generic;

namespace DragonMarble.Message
{
public enum GameMessageType
{
	RollMoveDiceResult,
	InitailizePlayer,
	OrderCardResult,
	OrderCardSelect,
	InitializeGame,
	RollMoveDice,
    InformOtherPlayerAction,
    ActivateTurn
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
		case GameMessageType.RollMoveDiceResult:
		message = new RollMoveDiceResultGameMessage();
		break;
		case GameMessageType.InitailizePlayer:
		message = new InitailizePlayerGameMessage();
		break;
		case GameMessageType.OrderCardResult:
		message = new OrderCardResultGameMessage();
		break;
		case GameMessageType.OrderCardSelect:
		message = new OrderCardSelectGameMessage();
		break;
		case GameMessageType.InitializeGame:
		message = new InitializeGameGameMessage();
		break;
		case GameMessageType.RollMoveDice:
		message = new RollMoveDiceGameMessage();
		break;
        case GameMessageType.InformOtherPlayerAction:
        message = new InformOtherPlayerActionGameMessage();
	    break;
        case GameMessageType.ActivateTurn:
        message = new ActivateTurnGameMessage();
        break;
	}
	return message;
}
}

    public class ActivateTurnGameMessage : IDragonMarbleGameMessage
    {
        public Int16 Length
        {
            get
            {
                return (Int16)(2 + (sizeof(GameMessageType)) + (16) + (16) + 16 + 64);
            }
        }
        public byte[] ToByteArray()
        {
            byte[] bytes = new byte[Length];
            int index = 0;
            BitConverter.GetBytes(Length)
            .CopyTo(bytes, index);
            index += sizeof(Int16);
            BitConverter.GetBytes((Int32)MessageType)
            .CopyTo(bytes, index);
            index += sizeof(GameMessageType);
            From.ToByteArray()
            .CopyTo(bytes, index);
            index += 16;
            To.ToByteArray()
            .CopyTo(bytes, index);
            index += 16;
            TurnOwner.ToByteArray()
            .CopyTo(bytes, index);
            index += 16;
            BitConverter.GetBytes(ResponseLimit)
            .CopyTo(bytes, index);
            index += 64;
            return bytes;
        }

        public void FromByteArray(byte[] bytes)
        {
            int index = 6;
            byte[] tempFrom = new byte[16]; Buffer.BlockCopy(bytes, index, tempFrom, 0, 16);
            From = new Guid(tempFrom);
            index += 16;
            byte[] tempTo = new byte[16]; Buffer.BlockCopy(bytes, index, tempTo, 0, 16);
            To = new Guid(tempTo);
            index += 16;
            byte[] tempTurnOwner = new byte[16]; Buffer.BlockCopy(bytes, index, tempTurnOwner, 0, 16);
            TurnOwner = new Guid(tempTurnOwner);
            index += 16;
            ResponseLimit = BitConverter.ToInt64(bytes, index);
            index += 64;
        }

        public GameMessageType MessageType { get { return GameMessageType.ActivateTurn; } }
        public Guid To { get; set; }
        public Guid From { get; set; }
        public Guid TurnOwner;
        public Int64 ResponseLimit;
    }

    public class InformOtherPlayerActionGameMessage : IDragonMarbleGameMessage
    {
        public GameMessageType MessageType { get { return GameMessageType.InformOtherPlayerAction; } }
    public Guid To { get; set; }
    public Guid From { get; set; }
    
    public IDragonMarbleGameMessage InformMessage;    
    public byte[] ToByteArray()
        {
            byte[] bytes = new byte[Length];
            int index = 0;
            BitConverter.GetBytes(Length)
            .CopyTo(bytes, index);
            index += sizeof(Int16);
            BitConverter.GetBytes((Int32)MessageType)
            .CopyTo(bytes, index);
            index += sizeof(GameMessageType);
            From.ToByteArray()
            .CopyTo(bytes, index);
            index += 16;
            To.ToByteArray()
            .CopyTo(bytes, index);
            index += 16;
            InformMessage.ToByteArray().CopyTo(bytes, index);
            index += InformMessage.Length;
            return bytes;
        }

        public void FromByteArray(byte[] bytes)
        {
            int index = 6;
            byte[] tempFrom = new byte[16]; Buffer.BlockCopy(bytes, index, tempFrom, 0, 16);
            From = new Guid(tempFrom);
            index += 16;
            byte[] tempTo = new byte[16]; Buffer.BlockCopy(bytes, index, tempTo, 0, 16);
            To = new Guid(tempTo);
            index += 16;
            byte[] tempInformMessage = new byte[InformMessage.Length];
            Buffer.BlockCopy(bytes, index, tempInformMessage, 0, InformMessage.Length);
            InformMessage.FromByteArray(tempInformMessage);
            index += InformMessage.Length;
        }

        public Int16 Length
        {
            get
            {
                return (Int16)(2 + (sizeof(GameMessageType)) + (16) + (16) + InformMessage.Length);
            }
        }
    }

// 서버에서 주사위 굴리기 결과	
public class RollMoveDiceResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDiceResult;}}
    public Guid To { get; set; }
    public Guid From { get; set; }

    public List<Char> Dices;

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
		index += sizeof(char);
		BitConverter.GetBytes(Dices[1])
		.CopyTo(bytes,index);
        index += sizeof(char);
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
	byte[] tempFrom = new byte[16];Buffer.BlockCopy(bytes, index,tempFrom,0,16);
		From = new Guid(tempFrom);
		index += 16;
	byte[] tempTo = new byte[16];Buffer.BlockCopy(bytes, index,tempTo,0,16);
		To = new Guid(tempTo);
		index += 16;
        Dices = new List<char>();
		Dices.Add(BitConverter.ToChar(bytes,index));
		index += sizeof(Char);
		Dices.Add(BitConverter.ToChar(bytes,index));
        index += sizeof(Char);
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16)+(2*(sizeof(Int32))));
	}
}
}

// 플레이어 초기화	
public class InitailizePlayerGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitailizePlayer;}}
    public Guid To { get; set; }
    public Guid From { get; set; }


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
	byte[] tempFrom = new byte[16];Buffer.BlockCopy(bytes, index,tempFrom,0,16);
		From = new Guid(tempFrom);
		index += 16;
	byte[] tempTo = new byte[16];Buffer.BlockCopy(bytes, index,tempTo,0,16);
		To = new Guid(tempTo);
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

// 선 뽑기 결과	
public class OrderCardResultGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OrderCardResult;}}
    public Guid To { get; set; }
    public Guid From { get; set; }

    public Int16 FirstCardNumber;
	public Int16 NumberOfPlayers;
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
		BitConverter.GetBytes(NumberOfPlayers)
		.CopyTo(bytes,index);
        index += sizeof(Int16);
    FirstPlayerId.ToByteArray()
    .CopyTo(bytes, index);
    index += 16;
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
	byte[] tempFrom = new byte[16];Buffer.BlockCopy(bytes, index,tempFrom,0,16);
		From = new Guid(tempFrom);
		index += 16;
	byte[] tempTo = new byte[16];Buffer.BlockCopy(bytes, index,tempTo,0,16);
		To = new Guid(tempTo);
		index += 16;
		FirstCardNumber = BitConverter.ToInt16(bytes,index);
		index += sizeof(Int16);
		NumberOfPlayers = BitConverter.ToInt16(bytes,index);
		index += sizeof(Int16);
    byte[] tempFirstPlayerId = new byte[16]; 
    Buffer.BlockCopy(bytes, index, tempFirstPlayerId, 0, 16);
    FirstPlayerId = new Guid(tempFirstPlayerId);
    index += 16;
}

public Int16 Length
{
	get
	{
	return (Int16)(16+2+(sizeof(GameMessageType))+(16)+(16)+(sizeof(Int16))+(sizeof(Int16)));
	}
}

    
}

// 선 뽑기	
public class OrderCardSelectGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.OrderCardSelect;}}
    public Guid To { get; set; }
    public Guid From { get; set; }

    public Int16 SelectedCardNumber;
	public Int16 NumberOfPlayers;
	public List<Boolean> OrderCardSelectState;

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
	return bytes;
}

public void FromByteArray(byte[] bytes)
{
		int index = 6;
	byte[] tempFrom = new byte[16];Buffer.BlockCopy(bytes, index,tempFrom,0,16);
		From = new Guid(tempFrom);
		index += 16;
	byte[] tempTo = new byte[16];Buffer.BlockCopy(bytes, index,tempTo,0,16);
		To = new Guid(tempTo);
		index += 16;
		SelectedCardNumber = BitConverter.ToInt16(bytes,index);
		index += sizeof(Int16);
		NumberOfPlayers = BitConverter.ToInt16(bytes,index);
		index += sizeof(Int16);
        OrderCardSelectState = new List<Boolean>();
	for (int i = 0; i < NumberOfPlayers ; i++ )
	{
        Boolean targetOrderCardSelectState = BitConverter.ToBoolean(bytes, index);
	    index += sizeof (Boolean);
		OrderCardSelectState.Add(targetOrderCardSelectState);
	}
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16)+(sizeof(Int16))+(sizeof(Int16))+(NumberOfPlayers*(sizeof(Boolean))));
	}
}
}

// 게임 초기화 정보	
public class InitializeGameGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.InitializeGame;}}
    public Guid To { get; set; }
    public Guid From { get; set; }

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
		BitConverter.GetBytes((Int32)Units[i].unitColor)
		.CopyTo(bytes,index);
		index += sizeof(StageUnitInfo.UNIT_COLOR);
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
    Buffer.BlockCopy(bytes, index,tempFrom,0,16);
		From = new Guid(tempFrom);
		index += 16;
	    byte[] tempTo = new byte[16];Buffer.BlockCopy(bytes, index,tempTo,0,16);
		To = new Guid(tempTo);
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
		index += sizeof(Int32);
		targetUnits.Order = BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
		targetUnits.Capital = BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
		targetUnits.unitColor = (StageUnitInfo.UNIT_COLOR)BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
		targetUnits.ControlMode = (StageUnitInfo.ControlModeType)BitConverter.ToInt32(bytes, index);
		index += sizeof(Int32);
        byte[] tempId = new byte[16];
        Buffer.BlockCopy(bytes, index, tempId, 0, 16);
        targetUnits.Id = new Guid(tempId);
        Console.WriteLine(targetUnits.Id);
        Console.WriteLine(tempId);
        Console.WriteLine(bytes);
		index += 16;
		Units.Add(targetUnits);
	}
}

public Int16 Length
{
	get
	{
	return (Int16)(2+(sizeof(GameMessageType))+(16)+(16)+(4*(sizeof(Int16)))+(sizeof(Int16))+(NumberOfPlayers*(sizeof(Int32)*5 + 16)));
	}
}
}

// 클라이언트에서 이동 주사위 굴리기	
public class RollMoveDiceGameMessage : IDragonMarbleGameMessage	
{
	public GameMessageType MessageType {get{return GameMessageType.RollMoveDice;}}
    public Guid To { get; set; }
    public Guid From { get; set; }

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
		int index = 6;
	byte[] tempFrom = new byte[16];Buffer.BlockCopy(bytes, index,tempFrom,0,16);
		From = new Guid(tempFrom);
		index += 16;
	byte[] tempTo = new byte[16];Buffer.BlockCopy(bytes, index,tempTo,0,16);
		To = new Guid(tempTo);
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

}