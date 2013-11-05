import _properties

def convert_from_bit(target, targetType, length, cast):
	result = ''
	if cast is not None:
		result += '\n\t\t%s = (%s)BitConverter.To%s(bytes,index);'%(target, targetType,cast)
	else:
		result += '\n\t\t%s = BitConverter.To%s(bytes,index);'%(target, targetType)
	result = result+'\n\t\tindex += %s;'%length
	return result

def initialize_collection(name, collection, target_type):
	return '\n\t\t%s = new %s<%s>();'%(name,collection,target_type)

def add_index(value):
	return '\n\t\tindex += %s;'%value

def add_object(target_name, value):
	return '\n\t\t%s.Add(%s);'%(target_name, value)

def bit_convert_to(field_name, field_type, cast_type, options, length):	
	result =''
	if 'from_byte_array' in options:		
		result += '\n\t\t%s = new %s();'%(field_name, field_type)
		result += '\n\t\tbyte[] temp%s = new byte[%s];'%(field_name, length)
		result += '\n\t\tBuffer.BlockCopy(bytes, index, temp%s, 0,%s);'%(field_name, length)
		result += '\n\t\t%s.FromByteArray(bytes);'%field_name
	elif 'byte_param_constructor' in options:
		field_name_no_dot = field_name.replace('.','_')
		result += '\n\t\tbyte[] temp%s = new byte[%s];'%(field_name_no_dot, length)
		result += '\n\t\tBuffer.BlockCopy(bytes, index, temp%s, 0,%s);'%(field_name_no_dot, length)
		result += '\n\t\t%s = new %s(temp%s);'%(field_name, field_type,field_name_no_dot)
	else:
		if field_type == 'Byte' or field_type == 'byte':			
			result += '\n\t\t%s = bytes[index];'%(field_name)
		elif cast_type == 'Byte' or cast_type == 'byte':			
			result += '\n\t\t%s = (%s)bytes[index];'%(field_name, field_type)
		elif cast_type is None:
			result += '\n\t\t%s = BitConverter.To%s(bytes, index);'%(field_name, field_type)		
		else:
			result += '\n\t\t%s = (%s)BitConverter.To%s(bytes, index);'%(field_name, field_type, cast_type)
	
	return result

def convert_from_one(field_name, field):
	result = ''	
	field_type = field['type']
	if 'targets' in field:
		result += '\n\t\t%s = new %s();'%(field_name, field_type)
		for target in field['targets']:
			result += convert_from_one('%s.%s'%(field_name,target['name']), target)
	else:
		cast_type = field.get('cast')
		#calculate_length has + in very front		
		length = _properties.calculate_length_one(field)
		result += bit_convert_to(field_name, field_type, cast_type, field.get('options',[]) , length)
		result += add_index(length)
	return result

def convert_from_one_for_collection(upper_field_name, field):
	result = ''	
	field_type = field['type']
	field_name = field['name']
	options = field.get('options')
	
	if 'targets' in field:				
		result += '\n\t\t%s temp%s = new %s();'%(field_type, field_name, field_type)
		for target in field['targets']:
			result += convert_from_one('temp%s.%s'%(field_name,target['name']), target)		
		result += '\n\t\t%s.Add(temp%s);'%(upper_field_name,field_name)
	else:
		cast_type = field.get('cast')
		#calculate_length has + in very front		
		length = _properties.calculate_length_one(field)
		converted = bit_convert_to(field_name, field_type, cast_type, field.get('options',[]) , length)
		converted = converted.replace(' = ','.Add(')
		converted = converted.replace(';',');')		
		result += converted
		result += add_index(length)
	return result

def convert(field):
	field_name = field['name']
	field_type = field['type']	
	options = field.get('options')	

	result = ''

	#make collection			
	if 'collection' in field:
		result += initialize_collection(field_name,field['collection'],field_type)		
		length = _properties.calculate_length_one(field)
		#target_object = 
		field_name = field['name']
		if type(field['size']) is int:
			for i in range(field['size']):				
				result +=  convert_from_one_for_collection(field_name,field)				
		else:
			result += '\n\t\tfor(int i = 0; i < %s; i++)\n\t\t{'%(field['size'])			
			result += convert_from_one_for_collection(field_name,field)
			result += '\n\t\t}'

		#result += add_object(field_name, convert_string ) 
		#result += add_index(length)
	else:
		result += convert_from_one(field_name, field)
	return result

def make_from_byte_array(f, fields):
	f.write('\n\npublic void FromByteArray(byte[] bytes)\n{')
	f.write('\n\t\tint index = 6;')
	
	for field in fields:
		field_name = field['name']
		#skip convert messageType
		if field_name == 'MessageType':
			continue
		f.write( convert(field) )

	f.write('\n}')