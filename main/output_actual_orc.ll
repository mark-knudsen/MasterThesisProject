; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global i64
@fmt_int = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_3() {
entry:
  store i64 9, ptr @x, align 8
  %inc_load = load i64, ptr @x, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @x, align 8
  %x = load i64, ptr @x, align 4
  %printcall = call i32 (ptr, ...) @printf(ptr @fmt_int, i64 %x)
  %dec_load = load i64, ptr @x, align 4
  %dec_sub = sub i64 %dec_load, 1
  store i64 %dec_sub, ptr @x, align 8
  %x1 = load i64, ptr @x, align 4
  %int_mem = call ptr @malloc(i64 8)
  store i64 %x1, ptr %int_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %int_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
