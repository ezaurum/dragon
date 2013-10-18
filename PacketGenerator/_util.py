def make_message_instance_maker(f, packet_list):
	f.write('\npublic static class GameMessageFactory')
	f.write('\n{')
	f.write('\n\tpublic static IDragonMarbleGameMessage GetGameMessage(byte[] bytes)')
	f.write('\n\t{')
	f.write('\n\t\tGameMessageType messageType = (GameMessageType) BitConverter.ToInt32(bytes, 2);')
	f.write('\n\t\tIDragonMarbleGameMessage message = GetGameMessage(messageType);')
	f.write('\n\t\tmessage.FromByteArray(bytes);')
	f.write('\n\t\treturn message;')
	f.write('\n\t}')
	f.write('\n\tpublic static IDragonMarbleGameMessage GetGameMessage(GameMessageType messageType)')
	f.write('\n\t{')
	f.write('\n\t\tIDragonMarbleGameMessage message = null;')
	f.write('\n\t\tswitch (messageType)')
	f.write('\n\t\t{')
	for packet_name in packet_list:
		f.write('\n\t\tcase GameMessageType.%s:'%packet_name)
		f.write('\n\t\tmessage = new %sGameMessage();'%packet_name)		
		f.write('\n\t\tbreak;')
	f.write('\n\t\t}')
	f.write('\n\t\treturn message;')
	f.write('\n\t}')
	f.write('\n}')

def make_message_types(packet_list):
	result = '\npublic enum GameMessageType\n{'
	for packet_name in packet_list:
		result += '\n\t%s,'%packet_name
	result += '\n}'
	return result