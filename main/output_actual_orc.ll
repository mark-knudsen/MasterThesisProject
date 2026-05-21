; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = global i64 0, align 8
@y = global i64 0, align 8
@z = global i64 0, align 8

define ptr @main_1() {
entry:
  store i64 5, ptr @x, align 8
  store i64 2, ptr @y, align 8
  %x_load = load i64, ptr @x, align 8
  %y_load = load i64, ptr @y, align 8
  %addtmp = add i64 %x_load, %y_load
  %addtmp1 = add i64 %addtmp, 1
  store i64 %addtmp1, ptr @z, align 8
  %z_load = load i64, ptr @z, align 8
  %value_mem = call ptr @malloc(i64 8)
  store i64 %z_load, ptr %value_mem, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 1, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
