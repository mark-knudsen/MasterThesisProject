; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr
@__where_src = external global ptr, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__where_result = external global ptr, align 8
@__where_i = external global i64, align 8

define ptr @main_34() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 32)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 2, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 4, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %arr_header2 = call ptr @malloc(i64 24)
  %arr_data3 = call ptr @malloc(i64 32)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 0
  store i64 0, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 1
  store i64 4, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 2
  store ptr %arr_data3, ptr %5, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %arr_data5 = call ptr @malloc(i64 32)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  store i64 2, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  store i64 4, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store ptr %arr_data5, ptr %8, align 8
  %elem_ptr6 = getelementptr ptr, ptr %arr_data5, i64 0
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr6, align 8
  %elem_ptr7 = getelementptr ptr, ptr %arr_data5, i64 1
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr7, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %9 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %9, align 8
  %10 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %arr_header2, ptr %10, align 8
  %11 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header4, ptr %11, align 8
  store ptr %df, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 8
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %rows_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load8 = load ptr, ptr @__where_src, align 8
  %__where_i_load9 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load8, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %in_bounds = icmp ult i64 %__where_i_load9, %len
  br i1 %in_bounds, label %idx_ok, label %idx_err

for.step:                                         ; preds = %ifcont
  %inc_load = load i64, ptr @__where_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load29 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr30 = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %__where_result_load29, ptr %data_ptr30, align 8
  ret ptr %runtime_obj

idx_ok:                                           ; preds = %for.body
  %elem_ptr10 = getelementptr ptr, ptr %data_ptr, i64 %__where_i_load9
  %record = load ptr, ptr %elem_ptr10, align 8
  %ptr_age = getelementptr ptr, ptr %record, i64 1
  %load_age_ptr = load ptr, ptr %ptr_age, align 8
  %val_age = load i64, ptr %load_age_ptr, align 4
  %icmp_tmp11 = icmp sgt i64 %val_age, 90
  br i1 %icmp_tmp11, label %then, label %else

idx_err:                                          ; preds = %for.body
  ret ptr null

then:                                             ; preds = %idx_ok
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load12 = load ptr, ptr @__where_src, align 8
  %__where_i_load13 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr14 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load12, i32 0, i32 1
  %rows15 = load ptr, ptr %rows_ptr_ptr14, align 8
  %data_ptr_ptr16 = getelementptr inbounds nuw %array, ptr %rows15, i32 0, i32 2
  %data_ptr17 = load ptr, ptr %data_ptr_ptr16, align 8
  %len_ptr18 = getelementptr inbounds nuw %array, ptr %rows15, i32 0, i32 0
  %len19 = load i64, ptr %len_ptr18, align 4
  %in_bounds20 = icmp ult i64 %__where_i_load13, %len19
  br i1 %in_bounds20, label %idx_ok21, label %idx_err22

else:                                             ; preds = %idx_ok
  br label %ifcont

ifcont:                                           ; preds = %else, %add_cont
  %iftmp = phi ptr [ %__where_result_load, %add_cont ], [ 0.000000e+00, %else ]
  br label %for.step

idx_ok21:                                         ; preds = %then
  %elem_ptr23 = getelementptr ptr, ptr %data_ptr17, i64 %__where_i_load13
  %record24 = load ptr, ptr %elem_ptr23, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr25 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len27 = load i64, ptr %len_ptr25, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr28 = load ptr, ptr %data_ptr_ptr26, align 8
  %is_full = icmp uge i64 %len27, %cap
  br i1 %is_full, label %grow, label %add_cont

idx_err22:                                        ; preds = %then
  ret ptr null

grow:                                             ; preds = %idx_ok21
  %12 = icmp eq i64 %cap, 0
  %13 = mul i64 %cap, 2
  %new_cap = select i1 %12, i64 4, i64 %13
  %new_byte_size = mul i64 %new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr28, i64 %new_byte_size)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr26, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %idx_ok21
  %final_data_ptr = phi ptr [ %data_ptr28, %idx_ok21 ], [ %realloc_ptr, %grow ]
  %target_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len27
  store ptr %record24, ptr %target_ptr, align 8
  %next_len = add i64 %len27, 1
  store i64 %next_len, ptr %len_ptr25, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
