; Packages
(package_clause (package_identifier) @name.package) @definition.package

; Functions 
(function_declaration name: (identifier) @name.function) @definition.function

; Methods 
(method_declaration name: (field_identifier) @name.method) @definition.method

; Interfaces 
(type_declaration (type_spec name: (type_identifier) @name.interface type: (interface_type))) @definition.interface

; Structs 
(type_declaration (type_spec name: name: (type_identifier) @name.struct type: (struct_type))) @definition.struct