; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global i64
@y = global i64 0
@z = global i64 0
@fmt_int = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_1() {
entry:
  store i64 20, ptr @x, align 8
  store i64 30, ptr @y, align 8
  %x_load = load i64, ptr @x, align 4
  %y_load = load i64, ptr @y, align 4
  %multmp = mul i64 %x_load, %y_load
  store i64 %multmp, ptr @z, align 8
  %z_load = load i64, ptr @z, align 4
  %printcall = call i32 (ptr, ptr, ...) @printf(ptr @fmt_int, i64 %z_load)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
