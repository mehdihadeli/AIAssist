; File-scoped Namespace 
(file_scoped_namespace_declaration
    name: (identifier)? @name.file_scoped_namespace name: (qualified_name)? @name.file_scoped_namespace) @definition.file_scoped_namespace

; Namespace declaration 
(namespace_declaration
    name: (qualified_name)? @name.namespace @definition.namespace  name: (identifier)? @name.namespace @definition.namespace) 

; Top-Level statement
(compilation_unit (global_statement)) @name.top_level_statement  @definition.top_level_statement 

; Enum declarations
(enum_declaration name: (identifier) @name.enum) @definition.enum

; Class declarations
(class_declaration name: (identifier) @name.class) @definition.class

; Interface declarations
(interface_declaration name: (identifier) @name.interface) @definition.interface

; Struct declarations
(struct_declaration name: (identifier) @name.struct) @definition.struct

; Record declarations
(record_declaration name: (identifier) @name.record) @definition.record

(method_declaration
            (modifier)? @modifier.method 
            returns: (type)? @return.method  
            name: (identifier) @name.method
            parameters: (parameter_list) @parameters.method
) @definition.method

(property_declaration 
        (modifier) @modifier.property 
        type: (predefined_type) @type.property
        name: (identifier) @name.property
) @definition.property
    