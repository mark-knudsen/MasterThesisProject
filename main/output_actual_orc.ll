; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr
@__where_src = global ptr null, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.2 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.3 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@__where_result = global ptr null, align 8
@__where_i = global i64 0, align 8

define ptr @main_12() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 32)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 4, ptr %len_ptr, align 8
  store i64 4, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data_raw, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data_raw, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data_raw, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 8
  %elem_ptr3 = getelementptr ptr, ptr %arr_data_raw, i64 3
  store ptr @str.3, ptr %elem_ptr3, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %arr_data_raw5 = call ptr @malloc(i64 800)
  %len_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  %cap_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  %data_field_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store i64 0, ptr %len_ptr6, align 8
  store i64 100, ptr %cap_ptr7, align 8
  store ptr %arr_data_raw5, ptr %data_field_ptr8, align 8
  %arr_header9 = call ptr @malloc(i64 24)
  %arr_data_raw10 = call ptr @malloc(i64 32)
  %len_ptr11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header9, i32 0, i32 0
  %cap_ptr12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header9, i32 0, i32 1
  %data_field_ptr13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header9, i32 0, i32 2
  store i64 4, ptr %len_ptr11, align 8
  store i64 4, ptr %cap_ptr12, align 8
  store ptr %arr_data_raw10, ptr %data_field_ptr13, align 8
  %elem_ptr14 = getelementptr i64, ptr %arr_data_raw10, i64 0
  store i64 4, ptr %elem_ptr14, align 8
  %elem_ptr15 = getelementptr i64, ptr %arr_data_raw10, i64 1
  store i64 1, ptr %elem_ptr15, align 8
  %elem_ptr16 = getelementptr i64, ptr %arr_data_raw10, i64 2
  store i64 3, ptr %elem_ptr16, align 8
  %elem_ptr17 = getelementptr i64, ptr %arr_data_raw10, i64 3
  store i64 2, ptr %elem_ptr17, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %0 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header, ptr %0, align 8
  %1 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %arr_header4, ptr %1, align 8
  %2 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header9, ptr %2, align 8
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
  %__where_src_load18 = load ptr, ptr @__where_src, align 8
  %__where_i_load19 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load18, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr20 = getelementptr ptr, ptr %data, i64 %__where_i_load19
  %record = load ptr, ptr %elem_ptr20, align 8
  %ptr_age = getelementptr ptr, ptr %record, i64 1
  %val_age = load i64, ptr %ptr_age, align 4
  %icmp_tmp21 = icmp sgt i64 %val_age, 50
  br i1 %icmp_tmp21, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load34 = load i64, ptr @__where_i, align 8
  %inc_add = add i64 %x_load34, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__where_result_load35 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__where_result_load35, ptr %data_ptr, align 8
  ret ptr %runtime_obj

then:                                             ; preds = %for.body
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load22 = load ptr, ptr @__where_src, align 8
  %__where_i_load23 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr24 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load22, i32 0, i32 1
  %rows25 = load ptr, ptr %rows_ptr_ptr24, align 8
  %data_ptr_ptr26 = getelementptr inbounds nuw %array, ptr %rows25, i32 0, i32 2
  %data27 = load ptr, ptr %data_ptr_ptr26, align 8
  %elem_ptr28 = getelementptr ptr, ptr %data27, i64 %__where_i_load23
  %record29 = load ptr, ptr %elem_ptr28, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr31 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr32 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr30, align 8
  %cap = load i64, ptr %cap_ptr31, align 8
  %data33 = load ptr, ptr %data_ptr_ptr32, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

else:                                             ; preds = %for.body
  br label %ifcont

ifcont:                                           ; preds = %else, %cont
  %iftmp = phi ptr [ %__where_result_load, %cont ], [ 0.000000e+00, %else ]
  br label %for.step

grow:                                             ; preds = %then
  %3 = icmp eq i64 %cap, 0
  %4 = mul i64 %cap, 2
  %new_cap = select i1 %3, i64 4, i64 %4
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data33, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr31, align 8
  store ptr %realloc, ptr %data_ptr_ptr32, align 8
  br label %cont

cont:                                             ; preds = %grow, %then
  %final_data_ptr = phi ptr [ %data33, %then ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %record29, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr30, align 8
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
