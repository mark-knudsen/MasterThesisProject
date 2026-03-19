; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@fmt_int_raw = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_1() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %len_ptr = getelementptr i64, ptr %arr_load, i32 0
  %array_len = load i64, ptr %len_ptr, align 4
  %item = alloca i64, align 8
  %foreach_counter = alloca i64, align 8
  store i64 0, ptr %foreach_counter, align 4
  br label %foreach.cond

foreach.cond:                                     ; preds = %foreach.body, %entry
  %cur_idx = load i64, ptr %foreach_counter, align 4
  %foreach_test = icmp slt i64 %cur_idx, %array_len
  br i1 %foreach_test, label %foreach.body, label %foreach.end

foreach.body:                                     ; preds = %foreach.cond
  %mem_idx = add i64 %cur_idx, 2
  %elem_ptr = getelementptr i64, ptr %arr_load, i64 %mem_idx
  %elem_val = load i64, ptr %elem_ptr, align 4
  store i64 %elem_val, ptr %item, align 4
  %item_load = load i64, ptr %item, align 4
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_int_raw, i64 %item_load)
  %next_idx = add i64 %cur_idx, 1
  store i64 %next_idx, ptr %foreach_counter, align 4
  br label %foreach.cond

foreach.end:                                      ; preds = %foreach.cond
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
