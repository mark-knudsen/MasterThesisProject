; ModuleID = 'repl_module'
source_filename = "repl_module"

@fmt_int_raw = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1
@fmt_int_raw.1 = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_17() {
entry:
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_int_raw, i64 8)
  %printf_call1 = call i32 (ptr, ...) @printf(ptr @fmt_int_raw.1, i64 0)
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
