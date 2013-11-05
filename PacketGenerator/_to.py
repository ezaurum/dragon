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

def convert_to(prefix, field_name,field, length,cast):
	result =''
	if 'targets' in field:
		if field['targets'] is None:
			print("field : %s, None targets"%field)

		result += inner_make(field_name + prefix+'.', field['targets'])
	else:
		target_string = field_name + prefix
		result += convert_to_bit(target_string, length,cast)
	return result

def inner_make(prefix, fields):
	result = ''	
	for field in fields:
		
		if field.get('type') is None:
			print("field %s has None type."%(field['name']))

		options = field.get('options',[])	
		length = field.get('length','sizeof(%s)'%field['type'])
		cast = field.get('cast')
		if prefix is None:
			prefix = ''
		field_name = prefix+field['name']
		if 'collection' in field:
			if 'size' not in field:
				print('Collection of %s, %s is not have size.'%(field['type'],field_name))
				break
			#when collection size is not int
			if type(field['size']) is not int:
				result += '\n\tfor (int i = 0; i < %s ; i++ )\n\t{'%field['size']
				result += convert_to('[i]',field_name,field, length,cast)
				result += '\n\t}'
			#when collection size is int
			else:
				for i in range(field['size']):
					result += convert_to('[%i]'%i, field_name, field, length, cast)	
		#when not collection
		else:
			target_string = field_name
			if 'to_byte_array' in options:
				result += convert_to_bit_by_self(target_string, length)
			else:
				result += convert_to_bit(target_string, length, cast)
	return result

def make_to_byte_array(fields):	
	result = '\n\n\tpublic byte[] ToByteArray()\n\t{'
	result += '\n\t\tbyte[] bytes = new byte[Length];\n\t\tint index = 0;'
	result += convert_to_bit('Length','sizeof(Int16)', None)
	result += inner_make(None, fields)

	result += '\n\treturn bytes;\n}'
	return result
