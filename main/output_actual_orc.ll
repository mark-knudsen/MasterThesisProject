; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr
@str = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__col_src = external global ptr, align 8
@__col_result = external global ptr, align 8
@__col_len = external global i64, align 8
@__col_i = external global i64, align 8
@df_idx_err_msg = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1
@__col_row = external global ptr, align 8

define ptr @main_6() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %x_load1 = load ptr, ptr @x, align 8
  store ptr %x_load1, ptr @__col_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 800)
  %arr_data = bitcast ptr %arr_data_raw to ptr
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %0, align 4
  store i64 100, ptr %1, align 4
  store ptr %arr_data, ptr %2, align 8
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
  %__col_src_load2 = load ptr, ptr @__col_src, align 8
  %__col_i_load3 = load i64, ptr @__col_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__col_src_load2, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_empty = icmp eq i64 %len, 0
  %in_bounds = icmp ult i64 %__col_i_load3, %len
  %3 = xor i1 %in_bounds, true
  %invalid = or i1 %is_empty, %3
  br i1 %invalid, label %df_idx_err, label %df_idx_ok

for.step:                                         ; preds = %cont
  %x_load8 = load i64, ptr @__col_i, align 8
  %inc_add = add i64 %x_load8, 1
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
  %elem_ptr = getelementptr ptr, ptr %data, i64 %__col_i_load3
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
  %ptr_age = getelementptr ptr, ptr %__col_row_load, i64 1
  %load_age_ptr = load ptr, ptr %ptr_age, align 8
  %val_age = load i64, ptr %load_age_ptr, align 4
  %len_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 1
  %data_ptr_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 2
  %len6 = load i64, ptr %len_ptr4, align 8
  %cap = load i64, ptr %cap_ptr, align 8
  %data7 = load ptr, ptr %data_ptr_ptr5, align 8
  %is_full = icmp uge i64 %len6, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge
  %4 = icmp eq i64 %cap, 0
  %5 = mul i64 %cap, 2
  %new_cap = select i1 %4, i64 4, i64 %5
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data7, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr, align 8
  store ptr %realloc, ptr %data_ptr_ptr5, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data7, %df_idx_merge ], [ %realloc, %grow ]
  %target = getelementptr i64, ptr %final_data_ptr, i64 %len6
  store i64 %val_age, ptr %target, align 4
  %new_len = add i64 %len6, 1
  store i64 %new_len, ptr %len_ptr4, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
