; ModuleID = 'repl_module'
source_filename = "repl_module"

@i = global i64 0
@fmt_int_raw = private unnamed_addr constant [5 x i8] c"%ld\0A\00", align 1

define ptr @main_5() {
entry:
  store i64 0, ptr @i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %i_load = load i64, ptr @i, align 4
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 24)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 3, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 3, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr bitcast (i64 2 to ptr), ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data, i64 2
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr2, align 8
  %len_ptr = getelementptr i64, ptr %arr_header, i32 0
  %length = load i64, ptr %len_ptr, align 8
  %icmp_tmp = icmp slt i64 %i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %i_load3 = load i64, ptr @i, align 4
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_int_raw, i64 %i_load3)
  br label %for.step

for.step:                                         ; preds = %for.body
  %inc_load = load i64, ptr @i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
