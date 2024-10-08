; Class declarations
(class_declaration name: (identifier) @class.name) @definition.class

; Base class references in class declarations
(class_declaration (base_list (_) @base_class.name)) @reference.base_class

; Interface declarations
(interface_declaration name: (identifier) @interface.name) @definition.interface

; Base interface references in interface declarations
(interface_declaration (base_list (_) @base_interface.name)) @reference.base_interface

; Method declarations
(method_declaration name: (identifier) @method.name) @definition.method

; AutoProperty declarations
(property_declaration name: (identifier) @auto_property.name) @definition.auto_property

; Object creation (instantiation)
(object_creation_expression type: (identifier) @instantiated_class.name) @reference.instantiated_class

; Type parameter constraints clause
(type_parameter_constraints_clause (identifier) @type_parameter_constraints_clause.name) @reference.type_parameter_constraints_clause

; Type parameter constraints (specific constraint type)
(type_parameter_constraint (type type: (identifier) @type_parameter_constraint.name)) @reference.type_parameter_constraint

; Variable declarations (referencing a type)
(variable_declaration type: (identifier) @variable_type.name) @reference.variable_type

; Method invocations (function calls)
(invocation_expression function: (member_access_expression name: (identifier) @called_method.name)) @reference.called_method

; Namespace declarations
(namespace_declaration name: (identifier) @namespace.name) @definition.namespace

; File-scoped Namespace declaration
(file_scoped_namespace_declaration (identifier) @file_scoped_namespace.name) @definition.file_scoped_namespace
 
; Namespace references
(namespace_declaration name: (identifier) @module.name) @reference.module

; Enum declarations
(enum_declaration name: (identifier) @enum.name) @definition.enum

; Struct declarations
(struct_declaration name: (identifier) @struct.name) @definition.struct

; Record declarations
(record_declaration name: (identifier) @record.name) @definition.record
