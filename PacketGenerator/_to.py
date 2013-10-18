# ToByteArray method implementation	
def convert_to_bit(target, length, cast):
	result = ''
	if cast is not None:
		result += '\n\t\tBitConverter.GetBytes((%s)%s)'%(cast,target)
	else:
		result += '\n\t\tBitConverter.GetBytes(%s)'%(target)
	result += '\n\t\t.CopyTo(bytes,index);'
	result += '\n\t\tindex += %s;'%length
	return result

def convert_to_bit_by_self(target, length):
	result = '\n\t\t%s.ToByteArray()'%(target)
	result += '\n\t\t.CopyTo(bytes,index);'
	result += '\n\t\tindex += %s;'%length
	return result

def convert_to_bit_inner_target(field, target):
	cast = target.get('cast')
	length = target.get('length','sizeof(%s)'%target['type'])
	target_string='%s[i].%s'%(field['name'],target['name'])
	return convert_to_bit(target_string, length,cast)

def make_to_byte_array(fields):	
	result = '\n\n\tpublic byte[] ToByteArray()\n\t{'
	result += '\n\t\tbyte[] bytes = new byte[Length];\n\t\tint index = 0;'
	result += convert_to_bit('Length','sizeof(Int16)', None)
	for field in fields:
		options = field.get('options',[])
		length = field.get('length','sizeof(%s)'%field['type'])
		cast = field.get('cast')
		if 'collection' in field:
			if 'size' not in field:
				print('Collection of %s, %s is not have size.'%(field['type'],field['name']))
				break
			#when collection size is not int
			if type(field['size']) is not int:
				result += '\n\tfor (int i = 0; i < %s ; i++ )\n\t{'%field['size']
				if 'target' in field:
					for target in field['target']:
						result += convert_to_bit_inner_target(field, target)
				else:
					target_string = '%s[i]'%field['name']
					result += convert_to_bit(target_string, length,cast)

				result += '\n\t}'
			#when collection size is int
			else:
				for i in range(field['size']):
					if 'target' in field:
						for target in field['target']:
							result += convert_to_bit_inner_target(field, target)
					else:
						target_string = '%s[%i]'%(field['name'],i)
						result += convert_to_bit(target_string, length, cast)
		#when not collection
		else:
			target_string = field['name']			
			if 'to_byte_array' in options:
				result += convert_to_bit_by_self(target_string, length)
			else:
				result += convert_to_bit(target_string, length, cast)

	print (result)

	result += '\n\treturn bytes;\n}'
	return result
