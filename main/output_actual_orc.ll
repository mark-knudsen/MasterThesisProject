; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = global ptr null
@fmt_array = private unnamed_addr constant [16 x i8] c"Array(len=%ld)\0A\00", align 1

define ptr @main_0() {
entry:
  %arr_ptr = call ptr @malloc(i64 64)
  %len_ptr = getelementptr i64, ptr %arr_ptr, i64 0
  store i64 6, ptr %len_ptr, align 8
  %cap_ptr = getelementptr i64, ptr %arr_ptr, i64 1
  store i64 6, ptr %cap_ptr, align 8
  %idx_0 = getelementptr i64, ptr %arr_ptr, i64 2
  store i64 -5, ptr %idx_0, align 8
  %idx_1 = getelementptr i64, ptr %arr_ptr, i64 3
  store i64 2, ptr %idx_1, align 8
  %idx_2 = getelementptr i64, ptr %arr_ptr, i64 4
  store i64 54, ptr %idx_2, align 8
  %idx_3 = getelementptr i64, ptr %arr_ptr, i64 5
  store i64 7, ptr %idx_3, align 8
  %idx_4 = getelementptr i64, ptr %arr_ptr, i64 6
  store i64 9, ptr %idx_4, align 8
  %idx_5 = getelementptr i64, ptr %arr_ptr, i64 7
  store i64 2, ptr %idx_5, align 8
  store ptr %arr_ptr, ptr @x, align 8
  %x_load = load ptr, ptr @x, align 8
  %arr_len = load i64, ptr %x_load, align 8
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_array, i64 %arr_len)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %x_load, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
