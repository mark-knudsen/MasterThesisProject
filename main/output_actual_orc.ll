; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = global ptr null, align 8
@y = global ptr null, align 8
@n = global i64 0, align 8
@sum_x = global i64 0, align 8
@sum_y = global i64 0, align 8
@i = global i64 0, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_0() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 64)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 4, ptr %len_ptr, align 8
  store i64 8, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr i64, ptr %arr_data_raw, i64 0
  store i64 1, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr i64, ptr %arr_data_raw, i64 1
  store i64 2, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr i64, ptr %arr_data_raw, i64 2
  store i64 3, ptr %elem_ptr2, align 8
  %elem_ptr3 = getelementptr i64, ptr %arr_data_raw, i64 3
  store i64 4, ptr %elem_ptr3, align 8
  store ptr %arr_header, ptr @x, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %arr_data_raw5 = call ptr @malloc(i64 64)
  %len_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  %cap_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  %data_field_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store i64 4, ptr %len_ptr6, align 8
  store i64 8, ptr %cap_ptr7, align 8
  store ptr %arr_data_raw5, ptr %data_field_ptr8, align 8
  %elem_ptr9 = getelementptr i64, ptr %arr_data_raw5, i64 0
  store i64 2, ptr %elem_ptr9, align 8
  %elem_ptr10 = getelementptr i64, ptr %arr_data_raw5, i64 1
  store i64 4, ptr %elem_ptr10, align 8
  %elem_ptr11 = getelementptr i64, ptr %arr_data_raw5, i64 2
  store i64 6, ptr %elem_ptr11, align 8
  %elem_ptr12 = getelementptr i64, ptr %arr_data_raw5, i64 3
  store i64 8, ptr %elem_ptr12, align 8
  store ptr %arr_header4, ptr @y, align 8
  %x_load = load ptr, ptr @x, align 8
  %length_ptr = getelementptr i64, ptr %x_load, i64 0
  %length = load i64, ptr %length_ptr, align 4
  store i64 %length, ptr @n, align 8
  store i64 0, ptr @sum_x, align 8
  store i64 0, ptr @sum_y, align 8
  store i64 0, ptr @i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %i_load = load i64, ptr @i, align 8
  %n_load = load i64, ptr @n, align 8
  %icmp_tmp = icmp slt i64 %i_load, %n_load
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %sum_x_load = load i64, ptr @sum_x, align 8
  %x_load13 = load ptr, ptr @x, align 8
  %i_load14 = load i64, ptr @i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load13, i32 0, i32 0
  %data_field_ptr15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load13, i32 0, i32 2
  %array_len = load i64, ptr %len_field_ptr, align 4
  %data_ptr = load ptr, ptr %data_field_ptr15, align 8
  %0 = icmp slt i64 %i_load14, 0
  %1 = icmp sge i64 %i_load14, %array_len
  %is_invalid = or i1 %0, %1
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %bounds.ok24
  %x_load29 = load i64, ptr @i, align 8
  %inc_add = add i64 %x_load29, 1
  store i64 %inc_add, ptr @i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr30 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr30, align 8
  ret ptr %runtime_obj

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr16 = getelementptr ptr, ptr %data_ptr, i64 %i_load14
  %raw_val = load ptr, ptr %elem_ptr16, align 8
  %2 = ptrtoint ptr %raw_val to i64
  %addtmp = add i64 %sum_x_load, %2
  store i64 %addtmp, ptr @sum_x, align 8
  %sum_y_load = load i64, ptr @sum_y, align 8
  %y_load = load ptr, ptr @y, align 8
  %i_load17 = load i64, ptr @i, align 8
  %len_field_ptr18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load, i32 0, i32 0
  %data_field_ptr19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load, i32 0, i32 2
  %array_len20 = load i64, ptr %len_field_ptr18, align 4
  %data_ptr21 = load ptr, ptr %data_field_ptr19, align 8
  %3 = icmp slt i64 %i_load17, 0
  %4 = icmp sge i64 %i_load17, %array_len20
  %is_invalid22 = or i1 %3, %4
  br i1 %is_invalid22, label %bounds.fail23, label %bounds.ok24

bounds.fail23:                                    ; preds = %bounds.ok
  %print_err25 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok24:                                      ; preds = %bounds.ok
  %elem_ptr26 = getelementptr ptr, ptr %data_ptr21, i64 %i_load17
  %raw_val27 = load ptr, ptr %elem_ptr26, align 8
  %5 = ptrtoint ptr %raw_val27 to i64
  %addtmp28 = add i64 %sum_y_load, %5
  store i64 %addtmp28, ptr @sum_y, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
