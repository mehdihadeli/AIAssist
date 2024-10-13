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

; Top-Level statement
(global_statement) @definition.top_level_statement 

; Enum declarations
(enum_declaration name: (identifier) @name.enum) @definition.enum

; Enum members list
(enum_declaration
  name: (identifier) @enum_name.member
  body: (enum_member_declaration_list
    (enum_member_declaration
      name: (identifier) @name.member
    )
  )
)

; Class declarations
(class_declaration name: (identifier) @name.class) @definition.class

; Struct declarations
(struct_declaration name: (identifier) @name.struct) @definition.struct

; Record declarations
(record_declaration name: (identifier) @name.record) @definition.record

; Interface declarations
(interface_declaration name: (identifier) @name.interface) @definition.interface

; Method parameters list
(method_declaration
  name: (identifier) @method_name.parameter
  parameters: (parameter_list
                (parameter
                  type: (type) @type.parameter
                  name: (identifier) @name.parameter))
) 

; Method declaration 
(method_declaration
  (modifier) @modifier.method 
  returns: (type) @return.method  
  name: (identifier) @name.method
   parameters: (parameter_list)
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
       