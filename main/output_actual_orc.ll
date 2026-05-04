; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@df = external global ptr
@__col_src = global ptr null, align 8
@__col_result = global ptr null, align 8
@__col_len = global i64 0, align 8
@__col_i = global i64 0, align 8
@df_idx_err_msg = private unnamed_addr constant [50 x i8] c"Runtime Error: Dataframe row index out of bounds\0A\00", align 1
@__col_row = global ptr null, align 8

define ptr @main_2() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__col_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 8)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %len_ptr, align 8
  store i64 0, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  store ptr %arr_header, ptr @__col_result, align 8
  %__col_src_load = load ptr, ptr @__col_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__col_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  store i64 %rows_length, ptr @__col_len, align 8
  store i64 0, ptr @__col_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__col_i_load = load i64, ptr @__col_i, align 8
  %__col_len_load = load i64, ptr @__col_len, align 8
  %icmp_tmp = icmp slt i64 %__col_i_load, %__col_len_load
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__col_src_load1 = load ptr, ptr @__col_src, align 8
  %__col_i_load2 = load i64, ptr @__col_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__col_src_load1, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr3 = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr3, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %in_bounds = icmp ult i64 %__col_i_load2, %len
  br i1 %in_bounds, label %df_idx_ok, label %df_idx_err

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__col_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__col_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__col_result_load9 = load ptr, ptr @__col_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 5, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__col_result_load9, ptr %data_ptr, align 8
  ret ptr %runtime_obj

df_idx_ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data, i64 %__col_i_load2
  %record = load ptr, ptr %elem_ptr, align 8
  br label %df_idx_merge

df_idx_err:                                       ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg)
  br label %df_idx_merge

df_idx_merge:                                     ; preds = %df_idx_ok, %df_idx_err
  %df_idx_result = phi ptr [ null, %df_idx_err ], [ %record, %df_idx_ok ]
  store ptr %df_idx_result, ptr @__col_row, align 8
  %__col_result_load = load ptr, ptr @__col_result, align 8
  %__col_row_load = load ptr, ptr @__col_row, align 8
  %ptr_latitude = getelementptr ptr, ptr %__col_row_load, i64 1
  %load_latitude_ptr = load ptr, ptr %ptr_latitude, align 8
  %val_latitude = load double, ptr %load_latitude_ptr, align 8
  %len_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 0
  %cap_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 1
  %data_ptr_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 2
  %len7 = load i64, ptr %len_ptr4, align 8
  %cap = load i64, ptr %cap_ptr5, align 8
  %data8 = load ptr, ptr %data_ptr_ptr6, align 8
  %is_full = icmp uge i64 %len7, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data8, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr5, align 8
  store ptr %realloc, ptr %data_ptr_ptr6, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data8, %df_idx_merge ], [ %realloc, %grow ]
  %target = getelementptr double, ptr %final_data_ptr, i64 %len7
  store double %val_latitude, ptr %target, align 8
  %new_len = add i64 %len7, 1
  store i64 %new_len, ptr %len_ptr4, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
