import yaml
import sys
import codecs
import _properties
import _util
import _from

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

def BitConvertAddTo(target, targetType, length, cast):
	if cast is not None:
		f.write('\n\t\t%s.Add((%s)BitConverter.To%s(bytes,index));'%(target, targetType,cast))
	else:
		f.write('\n\t\t%s.Add(BitConverter.To%s(bytes,index));'%(target, targetType))
	f.write('\n\t\tindex += %s;'%length)

def convertToBitInnerTarget(field, target):
	cast = target.get('cast')
	length = target.get('length','sizeof(%s)'%target['type'])
	target_string='%s[i].%s'%(field['name'],target['name'])
	convertToBit(target_string, length,cast)

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
f.write ( _util.make_message_types(packet_list) )

#make message instance maker
_util.make_message_instance_maker(f, packet_list)

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
	f.write( _properties.make_fields(packet_name, fields) )

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
	_from.make_from_byte_array(f, fields)

	# Length property getter implementation
	_properties.make_message_length(f, fields, length)
	
	#end of class
	f.write('\n}\n')

#end of namespace
f.write('\n}')
f.close()
print ('Generate Success.')
