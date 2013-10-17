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

def calculate_length(field):
	if field.get('targets') is None:
		options = field.get('options',[])
		length = field.get('length','sizeof(%s)'%(field['type']))		
		if 'has_length' in options:
			length = '%s.Length'%field['type']						
	else:
		length = ''
		for target in field.get('targets'):
			length += calculate_length(target)		
	
	if 'size' in field and 'collection' in field:
		if length[0] == '+' :
			length = length[1:]
		length = '+%s*(%s)'%(field['size'],length )
	else:
		length = '+%s'%(length)
	return length

#make length of message property
def make_message_length(f, fields, length):
	f.write('\n\n\tpublic Int16 Length')
	f.write('\n\t{')	
	f.write('\n\t\tget')
	f.write('\n\t\t{')
	#Int16 lengt is 2 bytes	
	f.write('\n\t\t\treturn (Int16)(')
	length = '2'
	for field in fields:
		length += calculate_length(field)
	
	f.write('%s);'%length)
	f.write('\n\t\t}')
	f.write('\n\t}')