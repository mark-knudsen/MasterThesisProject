; ModuleID = 'repl_module'
source_filename = "repl_module"

@i = global i64 0
@arr = external global ptr
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@fmt_int_raw = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_10() {
entry:
  store i64 0, ptr @i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %i_load = load i64, ptr @i, align 4
  %arr_load = load ptr, ptr @arr, align 8
  %len_ptr = getelementptr i64, ptr %arr_load, i32 0
  %length = load i64, ptr %len_ptr, align 8
  %icmp_tmp = icmp slt i64 %i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %arr_load1 = load ptr, ptr @arr, align 8
  %i_load2 = load i64, ptr @i, align 4
  %len_ptr3 = getelementptr i64, ptr %arr_load1, i32 0
  %array_len = load i64, ptr %len_ptr3, align 4
  %is_neg = icmp slt i64 %i_load2, 0
  %is_too_big = icmp sge i64 %i_load2, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %bounds.ok
  %inc_load = load i64, ptr @i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %offset_idx = add i64 %i_load2, 2
  %elem_ptr = getelementptr i64, ptr %arr_load1, i64 %offset_idx
  %raw_val = load i64, ptr %elem_ptr, align 8
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_int_raw, i64 %raw_val)
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
