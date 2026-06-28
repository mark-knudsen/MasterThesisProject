; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr

define ptr @main_15() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 6, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %x_load, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare noalias ptr @malloc(i64)
