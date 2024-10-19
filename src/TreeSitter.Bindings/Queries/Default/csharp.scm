; File-scoped Namespace 
(file_scoped_namespace_declaration
    name: (identifier)? @name.file_scoped_namespace name: (qualified_name)? @name.file_scoped_namespace)  @definition.file_scoped_namespace

; Namespace declaration 
(namespace_declaration
    name: (qualified_name)? @name.namespace  name: (identifier)? @name.namespace)  @definition.namespace

; Top-Level statement
(global_statement) @name.top_level_statement @definition.top_level_statement 

; Enum declarations
(enum_declaration name: (identifier) @name.enum) @definition.enum

; Class declarations
(class_declaration name: (identifier) @name.class) @definition.class

; Inherited types for a class
(class_declaration (base_list (_) @reference_name.class)) @reference.class

; Interface declarations
(interface_declaration name: (identifier) @name.interface) @definition.interface

; Inherited types for a interface
(interface_declaration (base_list (_) @reference_name.interface)) @reference.interface

; Struct declarations
(struct_declaration name: (identifier) @name.struct) @definition.struct

; Record declarations
(record_declaration name: (identifier) @name.record) @definition.record

(object_creation_expression type: (identifier) @reference_name.object_creation) @reference.object_creation

(type_parameter_constraints_clause (identifier) @reference_name.type_parameter_constraints_clause) @reference.type_parameter_constraints_clause

(type_parameter_constraint (type type: (identifier) @reference_name.type_parameter_constraint)) @reference.type_parameter_constraint

(variable_declaration type: (identifier) @reference_name.variable_declaration) @reference.variable_declaration

(invocation_expression function: (member_access_expression name: (identifier) @reference_name.invocation_expression)) @reference.invocation_expression

(method_declaration
            (modifier)? @modifier.method 
            returns: (type)? @return.method  
            name: (identifier) @name.method
            parameters: (parameter_list)
) @definition.method

(property_declaration 
        (modifier) @modifier.property 
        type: (predefined_type) @type.property
        name: (identifier) @name.property
) @definition.property
    
(field_declaration
    (variable_declaration
      type: (predefined_type) @type.field
      (variable_declarator
        name: (identifier) @name.field))
) @definition.field
