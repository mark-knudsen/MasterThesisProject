; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@df = external global ptr
@__map_src = global ptr null, align 8
@str = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.1 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@__map_result = global ptr null, align 8
@__map_i = global i64 0, align 8
@df_idx_err_msg = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1
@__current_row = global ptr null, align 8

define ptr @main_1() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 3200)
  %arr_data = bitcast ptr %arr_data_raw to ptr
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 2, ptr %0, align 4
  store i64 400, ptr %1, align 4
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %arr_header2 = call ptr @malloc(i64 24)
  %arr_data_raw3 = call ptr @malloc(i64 800)
  %arr_data4 = bitcast ptr %arr_data_raw3 to ptr
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 0
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 1
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 2
  store i64 0, ptr %3, align 4
  store i64 100, ptr %4, align 4
  store ptr %arr_data4, ptr %5, align 8
  %arr_header5 = call ptr @malloc(i64 24)
  %arr_data_raw6 = call ptr @malloc(i64 3200)
  %arr_data7 = bitcast ptr %arr_data_raw6 to ptr
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 0
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 1
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 2
  store i64 2, ptr %6, align 4
  store i64 400, ptr %7, align 4
  store ptr %arr_data7, ptr %8, align 8
  %elem_ptr8 = getelementptr i64, ptr %arr_data7, i64 0
  store i64 4, ptr %elem_ptr8, align 8
  %elem_ptr9 = getelementptr i64, ptr %arr_data7, i64 1
  store i64 4, ptr %elem_ptr9, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %9 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %9, align 8
  %10 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %arr_header2, ptr %10, align 8
  %11 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header5, ptr %11, align 8
  store ptr %df, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %rows_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_src_load10 = load ptr, ptr @__map_src, align 8
  %__map_i_load11 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load10, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_empty = icmp eq i64 %len, 0
  %in_bounds = icmp ult i64 %__map_i_load11, %len
  %12 = xor i1 %in_bounds, true
  %invalid = or i1 %is_empty, %12
  br i1 %invalid, label %df_idx_err, label %df_idx_ok

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__map_result_load21 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__map_result_load21, ptr %data_ptr, align 8
  ret ptr %runtime_obj

df_idx_ok:                                        ; preds = %for.body
  %elem_ptr12 = getelementptr ptr, ptr %data, i64 %__map_i_load11
  %record = load ptr, ptr %elem_ptr12, align 8
  br label %df_idx_merge

df_idx_err:                                       ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg)
  br label %df_idx_merge

df_idx_merge:                                     ; preds = %df_idx_ok, %df_idx_err
  %df_idx_result = phi ptr [ null, %df_idx_err ], [ %record, %df_idx_ok ]
  store ptr %df_idx_result, ptr @__current_row, align 8
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %record_buffer = call ptr @malloc(i64 16)
  %record_slots = bitcast ptr %record_buffer to ptr
  %__current_row_load = load ptr, ptr @__current_row, align 8
  %ptr_latitude = getelementptr ptr, ptr %__current_row_load, i64 1
  %load_latitude_ptr = load ptr, ptr %ptr_latitude, align 8
  %val_latitude = load double, ptr %load_latitude_ptr, align 8
  %field_mem = call ptr @malloc(i64 8)
  %cast = bitcast ptr %field_mem to ptr
  store double %val_latitude, ptr %cast, align 8
  %field_ptr = getelementptr ptr, ptr %record_slots, i64 0
  store ptr %field_mem, ptr %field_ptr, align 8
  %__current_row_load13 = load ptr, ptr @__current_row, align 8
  %ptr_longitude = getelementptr ptr, ptr %__current_row_load13, i64 2
  %load_longitude_ptr = load ptr, ptr %ptr_longitude, align 8
  %val_longitude = load double, ptr %load_longitude_ptr, align 8
  %field_mem14 = call ptr @malloc(i64 8)
  %cast15 = bitcast ptr %field_mem14 to ptr
  store double %val_longitude, ptr %cast15, align 8
  %field_ptr16 = getelementptr ptr, ptr %record_slots, i64 1
  store ptr %field_mem14, ptr %field_ptr16, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len19 = load i64, ptr %len_ptr17, align 8
  %cap = load i64, ptr %cap_ptr, align 8
  %data20 = load ptr, ptr %data_ptr_ptr18, align 8
  %is_full = icmp uge i64 %len19, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge
  %13 = icmp eq i64 %cap, 0
  %14 = mul i64 %cap, 2
  %new_cap = select i1 %13, i64 4, i64 %14
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data20, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr, align 8
  store ptr %realloc, ptr %data_ptr_ptr18, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data20, %df_idx_merge ], [ %realloc, %grow ]
  %target = getelementptr ptr, ptr %final_data_ptr, i64 %len19
  %15 = bitcast ptr %record_buffer to ptr
  store ptr %15, ptr %target, align 8
  %new_len = add i64 %len19, 1
  store i64 %new_len, ptr %len_ptr17, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
