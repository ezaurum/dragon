import yaml
import sys
import codecs

def convertToBit(target, length, cast):
	if cast is not None:
		f.write('\n\t\tBitConverter.GetBytes((%s)%s)'%(cast,target))
	else:
		f.write('\n\t\tBitConverter.GetBytes(%s)'%(target))
	f.write('\n\t\t.CopyTo(bytes,index);')
	f.write('\n\t\tindex += %s;'%length)

def convertToBitBySelf(target, length):
	f.write('\n\t\t%s.ToByteArray()'%(target))
	f.write('\n\t\t.CopyTo(bytes,index);')
	f.write('\n\t\tindex += %s;'%length)

def convertFromBit(target, targetType, length, cast):
	if cast is not None:
		f.write('\n\t\t%s = (%s)BitConverter.To%s(bytes,index);'%(target, targetType,cast))
	else:
		f.write('\n\t\t%s = BitConverter.To%s(bytes,index);'%(target, targetType))
	f.write('\n\t\tindex += %s;'%length)

def BitConvertAddTo(target, targetType, length, cast):
	if cast is not None:
		f.write('\n\t\t%s.Add((%s)BitConverter.To%s(bytes,index));'%(target, targetType,cast))
	else:
		f.write('\n\t\t%s.Add(BitConverter.To%s(bytes,index));'%(target, targetType))
	f.write('\n\t\tindex += %s;'%length)

def MakeCollection(name, collection, targetType):
	f.write('\n\t\t%s = new %s<%s>();'%(name,collection,targetType))

def convertToBitInnerTarget(field, target):
	cast = target.get('cast')
	length = target.get('length','sizeof(%s)'%target['type'])
	target_string='%s[i].%s'%(field['name'],target['name'])
	convertToBit(target_string, length,cast)

def makeMessageInstanceMaker(packet_list):
	f.write('\npublic static class GameMessageFactory')
	f.write('\n{')
	f.write('\npublic static IDragonMarbleGameMessage GetGameMessage(byte[] bytes)')
	f.write('\n{')
	f.write('\nGameMessageType messageType = (GameMessageType) BitConverter.ToInt32(bytes, 2);')
	f.write('\nIDragonMarbleGameMessage message = GetGameMessage(messageType);')
	f.write('\n\tmessage.FromByteArray(bytes);')
	f.write('\n\treturn message;')
	f.write('\n}')
	f.write('\npublic static IDragonMarbleGameMessage GetGameMessage(GameMessageType messageType)')
	f.write('\n{')
	f.write('\nIDragonMarbleGameMessage message = null;')
	f.write('\n\tswitch (messageType)')
	f.write('\n\t{')
	for packet_name in packet_list:
		f.write('\n\t\tcase GameMessageType.%s:'%packet_name)
		f.write('\n\t\tmessage = new %sGameMessage();'%packet_name)		
		f.write('\n\t\tbreak;')
	f.write('\n\t}')
	f.write('\n\treturn message;')
	f.write('\n}')
	f.write('\n}')

def makeTempByByteParamConstructor(name, targetType, length):
	f.write('\n\tbyte[] temp%s = new byte[%s];'%(name,length))
	f.write('Buffer.BlockCopy(bytes, index,temp%s,0,%s);'%(name,length))
	f.write('\n\t\t%s target%s = new %s(temp%s);'%(targetType, name, targetType, name))
	f.write('\n\t\tindex += %s;'%length)

def makeFieldByByteParamConstructor(name, targetType, length):
	f.write('\n\tbyte[] temp%s = new byte[%s];'%(name,length))
	f.write('Buffer.BlockCopy(bytes, index,temp%s,0,%s);'%(name,length))
	f.write('\n\t\t%s = new %s(temp%s);'%(name, targetType, name))
	f.write('\n\t\tindex += %s;'%length)
	
input_file = sys.argv[1]
output_file = sys.argv[2]
stream = open(input_file, 'r',encoding='utf-8')
packet_list = yaml.load(stream)
stream.close()
f = codecs.open(output_file, 'w', encoding='utf-8')
f.write('// Automatic generate by PacketGenerator.\n')
f.write('using System;\n')
f.write('using System.Collections.Generic;\n')
f.write('\nnamespace DragonMarble.Message')
f.write('\n{')

common_header = packet_list.get('Header',{});
if common_header is not None:
	del packet_list['Header']

#make messageType enum
f.write('\npublic enum GameMessageType\n{')
for packet_name in packet_list:
	f.write('\n\t%s,'%packet_name)
f.write('\n}')

#make message instance maker
makeMessageInstanceMaker(packet_list)

#make each message class
for packet_name in packet_list:
	packet = packet_list[packet_name]
	# Comment
	if packet.get('comment') is not None:
		f.write('\n// %s'%(packet['comment']))
	# class declare
	f.write('\t\npublic class %sGameMessage : IDragonMarbleGameMessage'%(packet_name))
	f.write('\t\n{')
	#header added
	fields = common_header.get('fields',[])+packet.get('fields',[])

	# class member
	for field in fields:
		#in case of collections e.g. List
		if 'collection' in field:
			f.write('\n\tpublic %s<%s> %s;'%(field['collection'],field['type'], field['name']))
		elif field['name'] == 'MessageType':			
			f.write('\n\tpublic GameMessageType MessageType {get{return GameMessageType.%s;}}'%packet_name)
		#in case of one object
		else:
			f.write('\n\tpublic %s %s;'%(field['type'], field['name']))

	# ToByteArray method implementation
	f.write('\n\n\tpublic byte[] ToByteArray()\n\t{')
	f.write('\n\t\tbyte[] bytes = new byte[Length];\n\t\tint index = 0;')
	convertToBit('Length','sizeof(Int16)', None)
	for field in fields:
		length = field.get('length','sizeof(%s)'%field['type'])
		cast = field.get('cast')
		if 'collection' in field:
			if 'size' not in field:
				print('Collection of %s, %s is not have size.'%(field['type'],field['name']))
				break
			#when collection size is not int
			if type(field['size']) is not int:
				f.write('\n\tfor (int i = 0; i < %s ; i++ )\n\t{'%field['size'])
				if 'target' in field:
					for target in field['target']:
						convertToBitInnerTarget(field, target)
				else:
					target_string = '%s[i]'%field['name']
					convertToBit(target_string, length,cast)

				f.write('\n\t}')
			#when collection size is int
			else:
				for i in range(field['size']):
					if 'target' in field:
						for target in field['target']:
							convertToBitInnerTarget(field, target)
					else:
						target_string = '%s[%i]'%(field['name'],i)
						convertToBit(target_string, length, cast)
		#when not collection
		else:
			target_string = field['name']
			if field.get('has_to_byte_array',False) is True:
				convertToBitBySelf(target_string, length)
			else:
				convertToBit(target_string, length, cast)

	f.write('\n\treturn bytes;\n}')

	# FromByteArray method implementation
	f.write('\n\npublic void FromByteArray(byte[] bytes)\n{')

	f.write('\n\t\tint index = 6;')
	for field in fields:
		if field['name'] == 'MessageType':
			continue
		length = field.get('length','sizeof(%s)'%field['type'])
		castType = field.get('cast')
		fieldType = field['type']
		if 'collection' in field:
			MakeCollection(field['name'],field['collection'],field['type'])
			if 'size' not in field:
				print('Collection of %s, %s is not have size.'%(field['type'],field['name']))
				break
			#when collection size is not int
			if type(field['size']) is not int:
				f.write('\n\t%s = new %s<%s>();'%(field['name'],field['collection'],field['type']))
				f.write('\n\tfor (int i = 0; i < %s ; i++ )\n\t{'%field['size'])
				#for object type has constructor
				if field.get('constructor') == 'byte_param':
					makeTempByByteParamConstructor(field['name'],field['type'],length)
					f.write('\n\t\t%s.Add(target%s);'%(field['name'],field['name']))
				#for object type don't has constructor
				elif field.get('constructor') == 'blank':
					f.write('\n\t\t%s target%s = new %s();'%(field['type'],field['name'],field['type']))
					if 'target' in field:
						for target in field['target']:
							cast = target.get('cast')
							if cast is None:
								f.write('\n\t\ttarget%s.%s = BitConverter.To%s(bytes, index);'%(field['name'],target['name'],target['type']))
							else:
								f.write('\n\t\ttarget%s.%s = (%s)BitConverter.To%s(bytes, index);'%(field['name'],target['name'],target['type'],target['cast']))
							f.write('\n\t\tindex += %s;'%length)
					f.write('\n\t\t%s.Add(target%s);'%(field['name'],field['name']))
				#for simple type
				else:
					cast = field.get('cast')
					if cast is None :
						f.write('\n\t\t%s target%s = BitConverter.To%s(bytes, index);'%(field['type'],field['name'],field['type']))
					else:
						f.write('\n\t\t%s target%s = (%s)BitConverter.To%s(bytes, index);'%(field['type'],field['name'],field['type'],cast))
					f.write('\n\t\t%s.Add(target%s);'%(field['name'],field['name']))

				f.write('\n\t}')

			#when collection size is int
			else:
				for i in range(field['size']):
					BitConvertAddTo(field['name'],fieldType,length, castType)

		#when not collection
		else:
			if field.get('constructor') == 'byte_param':
				makeFieldByByteParamConstructor(field['name'],field['type'],length)
			elif field.get('constructor') == 'blank':
				f.write('\n\t\t%s = new %s();'%(field['name'],fieldType))
				f.write('\n\t\tindex += %s;'%length)
			else:
				convertFromBit(field['name'],fieldType,length, castType)

	f.write('\n}')

	# Length property getter implementation
	f.write('\n\npublic Int16 Length\n{\n\tget\n\t{')
	f.write('\n\treturn (Int16)(2')
	for field in fields:
		length = 'sizeof(%s)'%(field['type'])		
		if 'size' in field:		
			if 'target' in field:
				l = len(field['target'])
				length = '(%s*(%s*%s))'%(field['size'],field.get('length',length),l)
			else:
				length = '(%s*(%s))'%(field['size'],field.get('length',length))
		else:
			length = '(%s)'%field.get('length',length)		
		f.write('+%s'%length)
	f.write(');')	
	f.write('\n\t}\n}')

	#end of class
	f.write('\n}\n')

#end of namespace
f.write('\n}')
f.close()
print ('Generate Success.')
