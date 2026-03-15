; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@fmt_str = private unnamed_addr constant [4 x i8] c"%s\0A\00", align 1

define ptr @main_4() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %len_ptr = getelementptr i64, ptr %arr_load, i32 0
  %array_len = load i64, ptr %len_ptr, align 4
  %is_too_big = icmp sge i64 0, %array_len
  %is_invalid = or i1 false, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

bounds.fail:                                      ; preds = %entry
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %entry
  %elem_ptr = getelementptr i64, ptr %arr_load, i64 1
  %raw_val = load i64, ptr %elem_ptr, align 8
  %to_str = inttoptr i64 %raw_val to ptr
  %printcall = call i32 (ptr, ptr, ...) @printf(ptr @fmt_str, ptr %to_str)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 4, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %to_str, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
