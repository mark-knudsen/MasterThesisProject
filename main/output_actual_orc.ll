; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr
@__map_src = global ptr null, align 8
@__map_result = global ptr null, align 8
@__map_i = global i64 0, align 8
@df_idx_err_msg = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1

define ptr @main_1() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 800)
  %arr_data = bitcast ptr %arr_data_raw to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %len_ptr, align 8
  store i64 100, ptr %cap_ptr, align 8
  store ptr %arr_data, ptr %data_field_ptr, align 8
  store ptr %arr_header, ptr @__map_result, align 8
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
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__map_src_load1 = load ptr, ptr @__map_src, align 8
  %__map_i_load2 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load1, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr3 = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr3, align 4
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_empty = icmp eq i64 %len, 0
  %in_bounds = icmp ult i64 %__map_i_load2, %len
  %0 = xor i1 %in_bounds, true
  %invalid = or i1 %is_empty, %0
  br i1 %invalid, label %df_idx_err, label %df_idx_ok

for.step:                                         ; preds = %cont
  %x_load9 = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load9, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__map_result_load10 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 5, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__map_result_load10, ptr %data_ptr, align 8
  ret ptr %runtime_obj

df_idx_ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data, i64 %__map_i_load2
  %record = load ptr, ptr %elem_ptr, align 8
  br label %df_idx_merge

df_idx_err:                                       ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg)
  br label %df_idx_merge

df_idx_merge:                                     ; preds = %df_idx_ok, %df_idx_err
  %df_idx_result = phi ptr [ null, %df_idx_err ], [ %record, %df_idx_ok ]
  %ptr_age = getelementptr ptr, ptr %df_idx_result, i64 1
  %load_age_ptr = load ptr, ptr %ptr_age, align 8
  %val_age = load i64, ptr %load_age_ptr, align 4
  %addtmp = add i64 %val_age, 10
  %len_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 0
  %cap_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 1
  %data_ptr_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 2
  %len7 = load i64, ptr %len_ptr4, align 8
  %cap = load i64, ptr %cap_ptr5, align 8
  %data8 = load ptr, ptr %data_ptr_ptr6, align 8
  %is_full = icmp uge i64 %len7, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge
  %1 = icmp eq i64 %cap, 0
  %2 = mul i64 %cap, 2
  %new_cap = select i1 %1, i64 4, i64 %2
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data8, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr5, align 8
  store ptr %realloc, ptr %data_ptr_ptr6, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data8, %df_idx_merge ], [ %realloc, %grow ]
  %target = getelementptr i64, ptr %final_data_ptr, i64 %len7
  store i64 %addtmp, ptr %target, align 4
  %new_len = add i64 %len7, 1
  store i64 %new_len, ptr %len_ptr4, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
