; File-scoped Namespace 
(file_scoped_namespace_declaration
    name: (identifier)? @name.file_scoped_namespace name: (qualified_name)? @name.file_scoped_namespace)  @definition.file_scoped_namespace

; Namespace declaration 
(namespace_declaration
    name: (qualified_name)? @name.namespace  name: (identifier)? @name.namespace)  @definition.namespace

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
(
    (comment)* @comment.class
    .
    (class_declaration 
        name: (identifier) @name.class
        body: (declaration_list
                    (property_declaration 
                        (modifier)? @modifier.property 
                        type: (predefined_type) @type.property
                        name: (identifier) @name.property
                    )* @definition.property
                    
                    (field_declaration
                        (variable_declaration
                          type: (predefined_type) @type.field
                          (variable_declarator
                            name: (identifier) @name.field))*
                    )* @definition.field
                 
                    (method_declaration
                        (modifier)? @modifier.method 
                        returns: (type)? @return.method  
                        name: (identifier)? @name.method
                        parameters: (parameter_list
                            (parameter
                                type: (type)? @type.parameter
                                name: (identifier)? @name.parameter
                            )
                        )* 
                    )? @definition.method
               )?
    ) @definition.class
)

; Struct declarations
(
    (comment)* @comment.struct
    .
    (struct_declaration 
        name: (identifier) @name.struct
        body: (declaration_list
                    (property_declaration 
                        (modifier)? @modifier.property 
                        type: (predefined_type) @type.property
                        name: (identifier) @name.property
                    )* @definition.property
                    
                    (field_declaration
                        (variable_declaration
                          type: (predefined_type) @type.field
                          (variable_declarator
                            name: (identifier) @name.field))
                    )* @definition.field
                    
                    (method_declaration
                        (modifier)? @modifier.method 
                        returns: (type)? @return.method  
                        name: (identifier) @name.method
                        parameters: (parameter_list
                            (parameter
                                type: (type)? @type.parameter
                                name: (identifier)? @name.parameter
                            )
                        )*
                    )? @definition.method
               )?
    ) @definition.struct
)


; Record declarations
(
    (comment)* @comment.record
    .
    (record_declaration 
        name: (identifier) @name.record
        body: (declaration_list
                    (property_declaration 
                        (modifier) @modifier.property 
                        type: (predefined_type) @type.property
                        name: (identifier) @name.property
                    )* @definition.property
                    
                    (field_declaration
                        (variable_declaration
                          type: (predefined_type) @type.field
                          (variable_declarator
                            name: (identifier) @name.field))
                    )* @definition.field
                    
          
                    (method_declaration
                        (modifier)? @modifier.method 
                        returns: (type)? @return.method  
                        name: (identifier) @name.method
                        parameters: (parameter_list
                            (parameter
                                type: (type)? @type.parameter
                                name: (identifier)? @name.parameter
                            )
                        )*
                    )? @definition.method
               )?
    ) @definition.record
)

; Interface declarations
(
    (comment)* @comment.interface
    .
    (interface_declaration 
        name: (identifier) @name.interface
        body: (declaration_list
                    (property_declaration 
                        (modifier) @modifier.property 
                        type: (predefined_type) @type.property
                        name: (identifier) @name.property
                    )* @definition.property
            
                    (method_declaration
                        (modifier)? @modifier.method 
                        returns: (type)? @return.method  
                        name: (identifier) @name.method
                        parameters: (parameter_list
                            (parameter
                                type: (type)? @type.parameter
                                name: (identifier)? @name.parameter
                            )
                        )*
                    )? @definition.method
               )?
    ) @definition.interface
)

