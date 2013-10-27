def make_message_instance_maker(packet_name_list):
	result = '\npublic static class GameMessageFactory'
	result += '\n{'
	result += '\n\tpublic static IDragonMarbleGameMessage GetGameMessage(byte[] bytes)'
	result += '\n\t{'
	result += '\n\t\tGameMessageType messageType = (GameMessageType) BitConverter.ToInt32(bytes, 2);'
	result += '\n\t\tIDragonMarbleGameMessage message = GetGameMessage(messageType);'
	result += '\n\t\tmessage.FromByteArray(bytes);'
	result += '\n\t\treturn message;'
	result += '\n\t}'
	result += '\n\tpublic static IDragonMarbleGameMessage GetGameMessage(GameMessageType messageType)'
	result += '\n\t{'
	result += '\n\t\tIDragonMarbleGameMessage message = null;'
	result += '\n\t\tswitch (messageType)'
	result += '\n\t\t{'
	for packet_name in packet_name_list:
		result += '\n\t\tcase GameMessageType.%s:'%packet_name
		result += '\n\t\tmessage = new %sGameMessage();'%packet_name	
		result += '\n\t\tbreak;'
	result += '\n\t\t}'
	result += '\n\t\treturn message;'
	result += '\n\t}'
	result += '\n}'
	return result

def make_message_types(packet_name_list):
	result = '\npublic enum GameMessageType\n{'
	for packet_name in packet_name_list:
		result += '\n\t%s,'%packet_name
	result += '\n}'
	return result