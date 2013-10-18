def calculate_length_one(field):
	length = ''
	if field.get('targets') is None:
		options = field.get('options',[])
		length = field.get('length','sizeof(%s)'%(field['type']))		
		if 'has_instance_length' in options:
			length = '%s.%s'%(field['name'],field.get('length','Length'))
		if 'has_type_length' in options:
			length = '%s.%s'%(field['type'],field.get('length','Length'))
	else:
		length = ''
		for target in field.get('targets'):
			length += calculate_length(target)
	return length

def calculate_length(field):
	length = calculate_length_one(field)
	
	if 'size' in field and 'collection' in field:
		if length[0] == '+' :
			length = length[1:]
		length = '+%s*(%s)'%(field['size'],length )
	else:
		length = '+%s'%(length)
	return length

#make length of message property
def make_message_length(f, fields):
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

#make fields
def make_fields(packet_name, fields):
	result =''
	for field in fields:
		options = field.get('options',[])
		#in case of collections e.g. List
		if 'collection' in field:
			result += '\n\tpublic %s<%s> %s;'%(field['collection'],field['type'], field['name'])
		elif field['name'] == 'MessageType':			
			result += '\n\tpublic GameMessageType MessageType {get{return GameMessageType.%s;}}'%packet_name
		#in case of one object
		elif 'property' in options:
			result += '\n\tpublic %s %s { get; set;}'%(field['type'], field['name'])	
		else:
			result += '\n\tpublic %s %s;'%(field['type'], field['name'])	
	return result