; File-scoped Namespace declaration multi part
(file_scoped_namespace_declaration
    name: (qualified_name) @name.file_scoped_namespace)  @definition.file_scoped_namespace

; File-scoped Namespace declaration single part
(file_scoped_namespace_declaration
    name: (identifier) @name.file_scoped_namespace)  @definition.file_scoped_namespace

; File-scoped Namespace declaration multi part
(namespace_declaration
    name: (qualified_name) @name.namespace)  @definition.namespace

; Namespace declarations single part
(namespace_declaration 
    name: (identifier) @name.namespace) @definition.namespace

; Enum declarations
(enum_declaration name: (identifier) @name.enum) @definition.enum

; Class declarations
(class_declaration name: (identifier) @name.class) @definition.class

; Struct declarations
(struct_declaration name: (identifier) @name.struct) @definition.struct

; Record declarations
(record_declaration name: (identifier) @name.record) @definition.record

; Interface declarations
(interface_declaration name: (identifier) @name.interface) @definition.interface

; Method declaration with return type
(method_declaration
  (modifier) @modifier.method 
  returns: (type) @return.method 
  name: (identifier) @name.method
  parameters: (parameter_list
                (parameter
                  type: (type) @parameter.type
                  name: (identifier) @parameter.name))
) @definition.method

; Method declaration without parameters and with return type
(method_declaration
  (modifier) @modifier.method 
  returns: (type) @return.method  
  name: (identifier) @name.method
) @definition.method

; Property declarations
(property_declaration 
  (modifier) @modifier.property 
   type: (predefined_type) @type.property
   name: (identifier) @name.property
)  @definition.property

; Field declarations
(field_declaration
    (variable_declaration
      type: (predefined_type) @type.field
      (variable_declarator
        name: (identifier) @name.field ))
) @definition.field
       