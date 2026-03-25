; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr

define ptr @main_4() {
entry:
  %runtime_obj_arr = load ptr, ptr @arr, align 8
  ret ptr %runtime_obj_arr
}

declare i32 @printf(ptr, ...)
