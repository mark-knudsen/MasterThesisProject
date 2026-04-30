; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr
@__map_src = external global ptr, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__map_result = external global ptr, align 8
@__map_i = external global i64, align 8
@df_idx_err_msg = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1
@__current_row = external global ptr, align 8
@str.2 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.3 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.4 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.5 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@df_idx_err_msg.6 = private unnamed_addr constant [64 x i8] c"Runtime Error: Cannot index dataframe (empty or out of bounds)\0A\00", align 1

define ptr @main_4() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 32)
  %arr_data = bitcast ptr %arr_data_raw to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 2, ptr %len_ptr, align 8
  store i64 4, ptr %cap_ptr, align 8
  store ptr %arr_data, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %arr_header2 = call ptr @malloc(i64 24)
  %arr_data_raw3 = call ptr @malloc(i64 800)
  %arr_data4 = bitcast ptr %arr_data_raw3 to ptr
  %len_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 0
  %cap_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 1
  %data_field_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 2
  store i64 0, ptr %len_ptr5, align 8
  store i64 100, ptr %cap_ptr6, align 8
  store ptr %arr_data4, ptr %data_field_ptr7, align 8
  %arr_header8 = call ptr @malloc(i64 24)
  %arr_data_raw9 = call ptr @malloc(i64 32)
  %arr_data10 = bitcast ptr %arr_data_raw9 to ptr
  %len_ptr11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 0
  %cap_ptr12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 1
  %data_field_ptr13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 2
  store i64 2, ptr %len_ptr11, align 8
  store i64 4, ptr %cap_ptr12, align 8
  store ptr %arr_data10, ptr %data_field_ptr13, align 8
  %elem_ptr14 = getelementptr i64, ptr %arr_data10, i64 0
  store i64 4, ptr %elem_ptr14, align 8
  %elem_ptr15 = getelementptr i64, ptr %arr_data10, i64 1
  store i64 4, ptr %elem_ptr15, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %0 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %0, align 8
  %1 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %arr_header2, ptr %1, align 8
  %2 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header8, ptr %2, align 8
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
  %__map_src_load16 = load ptr, ptr @__map_src, align 8
  %__map_i_load17 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load16, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr18 = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr18, align 4
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_empty = icmp eq i64 %len, 0
  %in_bounds = icmp ult i64 %__map_i_load17, %len
  %3 = xor i1 %in_bounds, true
  %invalid = or i1 %is_empty, %3
  br i1 %invalid, label %df_idx_err, label %df_idx_ok

for.step:                                         ; preds = %cont
  %x_load27 = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load27, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__map_result_load28 = load ptr, ptr @__map_result, align 8
  store ptr %__map_result_load28, ptr @__map_src, align 8
  %arr_header29 = call ptr @malloc(i64 24)
  %arr_data_raw30 = call ptr @malloc(i64 64)
  %arr_data31 = bitcast ptr %arr_data_raw30 to ptr
  %len_ptr32 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 0
  %cap_ptr33 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 1
  %data_field_ptr34 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 2
  store i64 4, ptr %len_ptr32, align 8
  store i64 8, ptr %cap_ptr33, align 8
  store ptr %arr_data31, ptr %data_field_ptr34, align 8
  %elem_ptr35 = getelementptr ptr, ptr %arr_data31, i64 0
  store ptr @str.2, ptr %elem_ptr35, align 8
  %elem_ptr36 = getelementptr ptr, ptr %arr_data31, i64 1
  store ptr @str.3, ptr %elem_ptr36, align 8
  %elem_ptr37 = getelementptr ptr, ptr %arr_data31, i64 2
  store ptr @str.4, ptr %elem_ptr37, align 8
  %elem_ptr38 = getelementptr ptr, ptr %arr_data31, i64 3
  store ptr @str.5, ptr %elem_ptr38, align 8
  %arr_header39 = call ptr @malloc(i64 24)
  %arr_data_raw40 = call ptr @malloc(i64 800)
  %arr_data41 = bitcast ptr %arr_data_raw40 to ptr
  %len_ptr42 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 0
  %cap_ptr43 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 1
  %data_field_ptr44 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 2
  store i64 0, ptr %len_ptr42, align 8
  store i64 100, ptr %cap_ptr43, align 8
  store ptr %arr_data41, ptr %data_field_ptr44, align 8
  %arr_header45 = call ptr @malloc(i64 24)
  %arr_data_raw46 = call ptr @malloc(i64 64)
  %arr_data47 = bitcast ptr %arr_data_raw46 to ptr
  %len_ptr48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header45, i32 0, i32 0
  %cap_ptr49 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header45, i32 0, i32 1
  %data_field_ptr50 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header45, i32 0, i32 2
  store i64 4, ptr %len_ptr48, align 8
  store i64 8, ptr %cap_ptr49, align 8
  store ptr %arr_data47, ptr %data_field_ptr50, align 8
  %elem_ptr51 = getelementptr i64, ptr %arr_data47, i64 0
  store i64 4, ptr %elem_ptr51, align 8
  %elem_ptr52 = getelementptr i64, ptr %arr_data47, i64 1
  store i64 4, ptr %elem_ptr52, align 8
  %elem_ptr53 = getelementptr i64, ptr %arr_data47, i64 2
  store i64 4, ptr %elem_ptr53, align 8
  %elem_ptr54 = getelementptr i64, ptr %arr_data47, i64 3
  store i64 4, ptr %elem_ptr54, align 8
  %df55 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %4 = getelementptr inbounds nuw %dataframe, ptr %df55, i32 0, i32 0
  store ptr %arr_header29, ptr %4, align 8
  %5 = getelementptr inbounds nuw %dataframe, ptr %df55, i32 0, i32 1
  store ptr %arr_header39, ptr %5, align 8
  %6 = getelementptr inbounds nuw %dataframe, ptr %df55, i32 0, i32 2
  store ptr %arr_header45, ptr %6, align 8
  store ptr %df55, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond56

df_idx_ok:                                        ; preds = %for.body
  %elem_ptr19 = getelementptr ptr, ptr %data, i64 %__map_i_load17
  %record = load ptr, ptr %elem_ptr19, align 8
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
  %ptr_name = getelementptr ptr, ptr %__current_row_load, i64 0
  %load_name_ptr = load ptr, ptr %ptr_name, align 8
  %field_ptr = getelementptr ptr, ptr %record_slots, i64 0
  store ptr %load_name_ptr, ptr %field_ptr, align 8
  %__current_row_load20 = load ptr, ptr @__current_row, align 8
  %ptr_age = getelementptr ptr, ptr %__current_row_load20, i64 1
  %load_age_ptr = load ptr, ptr %ptr_age, align 8
  %val_age = load i64, ptr %load_age_ptr, align 4
  %field_mem = call ptr @malloc(i64 8)
  %cast = bitcast ptr %field_mem to ptr
  store i64 %val_age, ptr %cast, align 8
  %field_ptr21 = getelementptr ptr, ptr %record_slots, i64 1
  store ptr %field_mem, ptr %field_ptr21, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr22 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr23 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len25 = load i64, ptr %len_ptr22, align 8
  %cap = load i64, ptr %cap_ptr23, align 8
  %data26 = load ptr, ptr %data_ptr_ptr24, align 8
  %is_full = icmp uge i64 %len25, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge
  %7 = icmp eq i64 %cap, 0
  %8 = mul i64 %cap, 2
  %new_cap = select i1 %7, i64 4, i64 %8
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data26, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr23, align 8
  store ptr %realloc, ptr %data_ptr_ptr24, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data26, %df_idx_merge ], [ %realloc, %grow ]
  %target = getelementptr ptr, ptr %final_data_ptr, i64 %len25
  %9 = bitcast ptr %record_buffer to ptr
  store ptr %9, ptr %target, align 8
  %new_len = add i64 %len25, 1
  store i64 %new_len, ptr %len_ptr22, align 8
  br label %for.step

for.cond56:                                       ; preds = %for.step58, %for.end
  %__map_i_load60 = load i64, ptr @__map_i, align 8
  %__map_src_load61 = load ptr, ptr @__map_src, align 8
  %rows_ptr_field62 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load61, i32 0, i32 1
  %rows_ptr63 = load ptr, ptr %rows_ptr_field62, align 8
  %rows_array_ptr64 = bitcast ptr %rows_ptr63 to ptr
  %rows_length_ptr65 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr64, i32 0, i32 0
  %rows_length66 = load i64, ptr %rows_length_ptr65, align 8
  %icmp_tmp67 = icmp slt i64 %__map_i_load60, %rows_length66
  br i1 %icmp_tmp67, label %for.body57, label %for.end59

for.body57:                                       ; preds = %for.cond56
  %__map_src_load68 = load ptr, ptr @__map_src, align 8
  %__map_i_load69 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr70 = getelementptr inbounds nuw %dataframe, ptr %__map_src_load68, i32 0, i32 1
  %rows71 = load ptr, ptr %rows_ptr_ptr70, align 8
  %len_ptr72 = getelementptr inbounds nuw %array, ptr %rows71, i32 0, i32 0
  %len73 = load i64, ptr %len_ptr72, align 4
  %data_ptr_ptr74 = getelementptr inbounds nuw %array, ptr %rows71, i32 0, i32 2
  %data75 = load ptr, ptr %data_ptr_ptr74, align 8
  %is_empty76 = icmp eq i64 %len73, 0
  %in_bounds77 = icmp ult i64 %__map_i_load69, %len73
  %10 = xor i1 %in_bounds77, true
  %invalid78 = or i1 %is_empty76, %10
  br i1 %invalid78, label %df_idx_err80, label %df_idx_ok79

for.step58:                                       ; preds = %cont115
  %x_load122 = load i64, ptr @__map_i, align 8
  %inc_add123 = add i64 %x_load122, 1
  store i64 %inc_add123, ptr @__map_i, align 8
  br label %for.cond56

for.end59:                                        ; preds = %for.cond56
  %__map_result_load124 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__map_result_load124, ptr %data_ptr, align 8
  ret ptr %runtime_obj

df_idx_ok79:                                      ; preds = %for.body57
  %elem_ptr83 = getelementptr ptr, ptr %data75, i64 %__map_i_load69
  %record84 = load ptr, ptr %elem_ptr83, align 8
  br label %df_idx_merge81

df_idx_err80:                                     ; preds = %for.body57
  %print_err82 = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg.6)
  br label %df_idx_merge81

df_idx_merge81:                                   ; preds = %df_idx_ok79, %df_idx_err80
  %df_idx_result85 = phi ptr [ null, %df_idx_err80 ], [ %record84, %df_idx_ok79 ]
  store ptr %df_idx_result85, ptr @__current_row, align 8
  %__map_result_load86 = load ptr, ptr @__map_result, align 8
  %record_buffer87 = call ptr @malloc(i64 32)
  %record_slots88 = bitcast ptr %record_buffer87 to ptr
  %__current_row_load89 = load ptr, ptr @__current_row, align 8
  %ptr_name90 = getelementptr ptr, ptr %__current_row_load89, i64 0
  %load_name_ptr91 = load ptr, ptr %ptr_name90, align 8
  %field_ptr92 = getelementptr ptr, ptr %record_slots88, i64 0
  store ptr %load_name_ptr91, ptr %field_ptr92, align 8
  %__current_row_load93 = load ptr, ptr @__current_row, align 8
  %ptr_name94 = getelementptr ptr, ptr %__current_row_load93, i64 0
  %load_name_ptr95 = load ptr, ptr %ptr_name94, align 8
  %field_ptr96 = getelementptr ptr, ptr %record_slots88, i64 1
  store ptr %load_name_ptr95, ptr %field_ptr96, align 8
  %__current_row_load97 = load ptr, ptr @__current_row, align 8
  %ptr_name98 = getelementptr ptr, ptr %__current_row_load97, i64 0
  %load_name_ptr99 = load ptr, ptr %ptr_name98, align 8
  %field_ptr100 = getelementptr ptr, ptr %record_slots88, i64 2
  store ptr %load_name_ptr99, ptr %field_ptr100, align 8
  %__current_row_load101 = load ptr, ptr @__current_row, align 8
  %ptr_name102 = getelementptr ptr, ptr %__current_row_load101, i64 0
  %load_name_ptr103 = load ptr, ptr %ptr_name102, align 8
  %field_ptr104 = getelementptr ptr, ptr %record_slots88, i64 3
  store ptr %load_name_ptr103, ptr %field_ptr104, align 8
  %rows_field105 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load86, i32 0, i32 1
  %rows_array106 = load ptr, ptr %rows_field105, align 8
  %len_ptr107 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array106, i32 0, i32 0
  %cap_ptr108 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array106, i32 0, i32 1
  %data_ptr_ptr109 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array106, i32 0, i32 2
  %len110 = load i64, ptr %len_ptr107, align 8
  %cap111 = load i64, ptr %cap_ptr108, align 8
  %data112 = load ptr, ptr %data_ptr_ptr109, align 8
  %is_full113 = icmp uge i64 %len110, %cap111
  br i1 %is_full113, label %grow114, label %cont115

grow114:                                          ; preds = %df_idx_merge81
  %11 = icmp eq i64 %cap111, 0
  %12 = mul i64 %cap111, 2
  %new_cap116 = select i1 %11, i64 4, i64 %12
  %bytes117 = mul i64 %new_cap116, 8
  %realloc118 = call ptr @realloc(ptr %data112, i64 %bytes117)
  store i64 %new_cap116, ptr %cap_ptr108, align 8
  store ptr %realloc118, ptr %data_ptr_ptr109, align 8
  br label %cont115

cont115:                                          ; preds = %grow114, %df_idx_merge81
  %final_data_ptr119 = phi ptr [ %data112, %df_idx_merge81 ], [ %realloc118, %grow114 ]
  %target120 = getelementptr ptr, ptr %final_data_ptr119, i64 %len110
  %13 = bitcast ptr %record_buffer87 to ptr
  store ptr %13, ptr %target120, align 8
  %new_len121 = add i64 %len110, 1
  store i64 %new_len121, ptr %len_ptr107, align 8
  br label %for.step58
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
