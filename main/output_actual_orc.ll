; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global i64

define ptr @main_9() {
entry:
  store i64 12, ptr @x, align 8
  ret { i32, ptr } undef
}

declare i32 @printf(ptr, ...)
