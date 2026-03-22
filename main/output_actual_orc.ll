; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr

define ptr @main_23() {
entry:
  %actual_ptr = load ptr, ptr @arr, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %actual_ptr, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
