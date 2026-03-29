; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global i64

define ptr @main_33() {
entry:
  %x_load = load i64, ptr @x, align 4
  %value_mem = call ptr @malloc(i64 8)
  store i64 %x_load, ptr %value_mem, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
